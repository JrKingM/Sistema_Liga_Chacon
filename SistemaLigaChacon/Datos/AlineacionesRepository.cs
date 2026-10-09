using System;
using System.Data;
using System.Data.SqlClient;

namespace SistemaLigaChacon.Datos
{
    /// <summary>Guarda y consulta las alineaciones especificas de cada partido.</summary>
    public static class AlineacionesRepository
    {
        public static DataTable ObtenerCompetencias()
        {
            const string sql = @"
SELECT c.CompetenciaId,
       ca.Nombre + N' - ' + t.Nombre AS Descripcion
FROM dbo.Competencias c
INNER JOIN dbo.Categorias ca ON ca.CategoriaId = c.CategoriaId
INNER JOIN dbo.Temporadas t ON t.TemporadaId = c.TemporadaId
ORDER BY t.TemporadaId DESC, ca.Nombre;";
            return Consultar(sql, null);
        }

        public static DataTable ObtenerPartidos(int competenciaId)
        {
            const string sql = @"
SELECT p.PartidoId,
       CONCAT(N'J', j.Numero, N' | ',
              FORMAT(p.FechaHora, 'dd/MM/yyyy HH:mm'), N' | ',
              local.Nombre, N' - ', visita.Nombre) AS Descripcion
FROM dbo.Partidos p
INNER JOIN dbo.Jornadas j ON j.JornadaId = p.JornadaId
INNER JOIN dbo.EquipoInscripciones eil
    ON eil.InscripcionEquipoId = p.EquipoLocalInscripcionId
INNER JOIN dbo.Equipos local ON local.EquipoId = eil.EquipoId
INNER JOIN dbo.EquipoInscripciones eiv
    ON eiv.InscripcionEquipoId = p.EquipoVisitaInscripcionId
INNER JOIN dbo.Equipos visita ON visita.EquipoId = eiv.EquipoId
WHERE p.CompetenciaId = @CompetenciaId
  AND p.Estado <> 'CANCELADO'
ORDER BY p.FechaHora DESC, j.Numero;";
            return Consultar(sql, comando =>
                comando.Parameters.Add("@CompetenciaId", SqlDbType.Int)
                    .Value = competenciaId);
        }

        public static DataTable ObtenerEquipos(int partidoId)
        {
            const string sql = @"
SELECT ei.InscripcionEquipoId, e.Nombre
FROM dbo.Partidos p
INNER JOIN dbo.EquipoInscripciones ei
    ON ei.InscripcionEquipoId = p.EquipoLocalInscripcionId
INNER JOIN dbo.Equipos e ON e.EquipoId = ei.EquipoId
WHERE p.PartidoId = @PartidoId
UNION ALL
SELECT ei.InscripcionEquipoId, e.Nombre
FROM dbo.Partidos p
INNER JOIN dbo.EquipoInscripciones ei
    ON ei.InscripcionEquipoId = p.EquipoVisitaInscripcionId
INNER JOIN dbo.Equipos e ON e.EquipoId = ei.EquipoId
WHERE p.PartidoId = @PartidoId;";
            return Consultar(sql, comando =>
                comando.Parameters.Add("@PartidoId", SqlDbType.Int).Value = partidoId);
        }

        public static DataTable ObtenerJugadores(int inscripcionEquipoId)
        {
            const string sql = @"
SELECT ij.InscripcionJugadorId,
       CONCAT(j.Nombres, N' ', j.Apellidos) AS Nombre
FROM dbo.InscripcionesJugadores ij
INNER JOIN dbo.Jugadores j ON j.JugadorId = ij.JugadorId
WHERE ij.InscripcionEquipoId = @EquipoId
  AND ij.Estado = 'ACTIVO'
  AND j.Estado = 'ACTIVO'
ORDER BY j.Apellidos, j.Nombres;";
            return Consultar(sql, comando =>
                comando.Parameters.Add("@EquipoId", SqlDbType.Int)
                    .Value = inscripcionEquipoId);
        }

        public static DataTable ObtenerAlineacion(
            int partidoId,
            int inscripcionEquipoId)
        {
            const string sql = @"
SELECT pa.Tipo, pa.Posicion, pa.EsPortero, ij.Dorsal,
       j.Nombres, j.Apellidos
FROM dbo.PartidoAlineaciones pa
INNER JOIN dbo.InscripcionesJugadores ij
    ON ij.InscripcionJugadorId = pa.InscripcionJugadorId
INNER JOIN dbo.Jugadores j ON j.JugadorId = ij.JugadorId
WHERE pa.PartidoId = @PartidoId
  AND pa.InscripcionEquipoId = @EquipoId
ORDER BY CASE pa.Tipo WHEN 'TITULAR' THEN 0 ELSE 1 END,
         pa.EsPortero DESC, ij.Dorsal, j.Apellidos, j.Nombres;";
            return Consultar(sql, comando =>
            {
                comando.Parameters.Add("@PartidoId", SqlDbType.Int).Value = partidoId;
                comando.Parameters.Add("@EquipoId", SqlDbType.Int)
                    .Value = inscripcionEquipoId;
            });
        }

        public static void GuardarJugadorEnPartido(
            int partidoId,
            int inscripcionEquipoId,
            int inscripcionJugadorId,
            string tipo,
            string posicion,
            bool esPortero)
        {
            if (tipo != "TITULAR" && tipo != "SUPLENTE")
                throw new ArgumentException("Selecciona titular o suplente.");

            const string sql = @"
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF NOT EXISTS (
    SELECT 1
    FROM dbo.Partidos p WITH (UPDLOCK, HOLDLOCK)
    WHERE p.PartidoId = @PartidoId
      AND p.Estado <> 'CANCELADO'
      AND @EquipoId IN (
          p.EquipoLocalInscripcionId,
          p.EquipoVisitaInscripcionId))
    THROW 50030, 'El equipo no participa en ese partido.', 1;

IF NOT EXISTS (
    SELECT 1
    FROM dbo.InscripcionesJugadores ij
    INNER JOIN dbo.Jugadores j ON j.JugadorId = ij.JugadorId
    WHERE ij.InscripcionJugadorId = @JugadorId
      AND ij.InscripcionEquipoId = @EquipoId
      AND ij.Estado = 'ACTIVO'
      AND j.Estado = 'ACTIVO')
    THROW 50031, 'El jugador no pertenece a la plantilla activa.', 1;

IF @Tipo = 'TITULAR'
   AND (SELECT COUNT(*)
        FROM dbo.PartidoAlineaciones WITH (UPDLOCK, HOLDLOCK)
        WHERE PartidoId = @PartidoId
          AND InscripcionEquipoId = @EquipoId
          AND Tipo = 'TITULAR'
          AND InscripcionJugadorId <> @JugadorId) >= 11
    THROW 50032, 'El equipo ya tiene 11 titulares en este partido.', 1;

UPDATE dbo.PartidoAlineaciones
SET InscripcionEquipoId = @EquipoId,
    Tipo = @Tipo,
    Posicion = @Posicion,
    EsPortero = @EsPortero
WHERE PartidoId = @PartidoId
  AND InscripcionJugadorId = @JugadorId;

IF @@ROWCOUNT = 0
BEGIN
    INSERT dbo.PartidoAlineaciones
        (PartidoId, InscripcionEquipoId, InscripcionJugadorId,
         Tipo, Posicion, EsPortero)
    VALUES
        (@PartidoId, @EquipoId, @JugadorId,
         @Tipo, @Posicion, @EsPortero);
END;

COMMIT TRANSACTION;";

            using (var conexion = ConexionBD.CrearConexion())
            using (var comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.Add("@PartidoId", SqlDbType.Int).Value = partidoId;
                comando.Parameters.Add("@EquipoId", SqlDbType.Int)
                    .Value = inscripcionEquipoId;
                comando.Parameters.Add("@JugadorId", SqlDbType.Int)
                    .Value = inscripcionJugadorId;
                comando.Parameters.Add("@Tipo", SqlDbType.VarChar, 12).Value = tipo;
                comando.Parameters.Add("@Posicion", SqlDbType.NVarChar, 30).Value =
                    String.IsNullOrWhiteSpace(posicion)
                        ? (object)DBNull.Value
                        : posicion.Trim();
                comando.Parameters.Add("@EsPortero", SqlDbType.Bit).Value = esPortero;
                conexion.Open();
                comando.ExecuteNonQuery();
            }
        }

        private static DataTable Consultar(
            string sql,
            Action<SqlCommand> agregarParametros)
        {
            var tabla = new DataTable();
            using (var conexion = ConexionBD.CrearConexion())
            using (var comando = new SqlCommand(sql, conexion))
            using (var adaptador = new SqlDataAdapter(comando))
            {
                if (agregarParametros != null)
                    agregarParametros(comando);

                adaptador.Fill(tabla);
            }

            return tabla;
        }
    }
}
