using System;
using System.Data;
using System.Data.SqlClient;

namespace SistemaLigaChacon.Datos
{
    /// <summary>Consulta partidos y jugadores y registra tarjetas del campeonato.</summary>
    public static class SancionesRepository
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
              el.Nombre, N' - ', ev.Nombre) AS Descripcion
FROM dbo.Partidos p
INNER JOIN dbo.Jornadas j ON j.JornadaId = p.JornadaId
INNER JOIN dbo.EquipoInscripciones eil
    ON eil.InscripcionEquipoId = p.EquipoLocalInscripcionId
INNER JOIN dbo.Equipos el ON el.EquipoId = eil.EquipoId
INNER JOIN dbo.EquipoInscripciones eiv
    ON eiv.InscripcionEquipoId = p.EquipoVisitaInscripcionId
INNER JOIN dbo.Equipos ev ON ev.EquipoId = eiv.EquipoId
WHERE p.CompetenciaId = @CompetenciaId
  AND p.Estado IN ('EN_CURSO', 'FINALIZADO')
ORDER BY p.FechaHora DESC, j.Numero;";

            return Consultar(sql, comando =>
                comando.Parameters.Add("@CompetenciaId", SqlDbType.Int)
                    .Value = competenciaId);
        }

        public static DataTable ObtenerEquiposDelPartido(int partidoId)
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
WHERE ij.InscripcionEquipoId = @InscripcionEquipoId
  AND ij.Estado = 'ACTIVO'
  AND j.Estado = 'ACTIVO'
ORDER BY j.Apellidos, j.Nombres;";

            return Consultar(sql, comando =>
                comando.Parameters.Add("@InscripcionEquipoId", SqlDbType.Int)
                    .Value = inscripcionEquipoId);
        }

        public static DataTable ObtenerSanciones(int competenciaId)
        {
            const string sql = @"
SELECT ev.EventoId, ev.Tipo, ev.Minuto, ev.Descripcion,
       p.FechaHora, jor.Numero AS Jornada,
       CONCAT(j.Nombres, N' ', j.Apellidos) AS Jugador,
       e.Nombre AS Equipo,
       CONCAT(local.Nombre, N' - ', visita.Nombre) AS Partido
FROM dbo.EventosPartido ev
INNER JOIN dbo.Partidos p ON p.PartidoId = ev.PartidoId
INNER JOIN dbo.Jornadas jor ON jor.JornadaId = p.JornadaId
INNER JOIN dbo.InscripcionesJugadores ij
    ON ij.InscripcionJugadorId = ev.InscripcionJugadorId
INNER JOIN dbo.Jugadores j ON j.JugadorId = ij.JugadorId
INNER JOIN dbo.EquipoInscripciones ei
    ON ei.InscripcionEquipoId = ev.InscripcionEquipoId
INNER JOIN dbo.Equipos e ON e.EquipoId = ei.EquipoId
INNER JOIN dbo.EquipoInscripciones eil
    ON eil.InscripcionEquipoId = p.EquipoLocalInscripcionId
INNER JOIN dbo.Equipos local ON local.EquipoId = eil.EquipoId
INNER JOIN dbo.EquipoInscripciones eiv
    ON eiv.InscripcionEquipoId = p.EquipoVisitaInscripcionId
INNER JOIN dbo.Equipos visita ON visita.EquipoId = eiv.EquipoId
WHERE p.CompetenciaId = @CompetenciaId
  AND ev.Tipo IN ('AMARILLA', 'ROJA')
ORDER BY p.FechaHora DESC, ev.Minuto, ev.EventoId DESC;";

            return Consultar(sql, comando =>
                comando.Parameters.Add("@CompetenciaId", SqlDbType.Int)
                    .Value = competenciaId);
        }

        public static void RegistrarSancion(
            int partidoId,
            int inscripcionEquipoId,
            int inscripcionJugadorId,
            string tipo,
            short? minuto,
            string descripcion)
        {
            if (tipo != "AMARILLA" && tipo != "ROJA")
                throw new ArgumentException("Selecciona tarjeta amarilla o roja.");

            const string sql = @"
INSERT dbo.EventosPartido
    (PartidoId, InscripcionEquipoId, InscripcionJugadorId,
     Tipo, Minuto, Descripcion)
SELECT @PartidoId, @EquipoId, @JugadorId,
       @Tipo, @Minuto, @Descripcion
WHERE EXISTS (
    SELECT 1
    FROM dbo.Partidos p
    WHERE p.PartidoId = @PartidoId
      AND p.Estado IN ('EN_CURSO', 'FINALIZADO')
      AND @EquipoId IN (
          p.EquipoLocalInscripcionId,
          p.EquipoVisitaInscripcionId))
  AND EXISTS (
    SELECT 1
    FROM dbo.InscripcionesJugadores ij
    WHERE ij.InscripcionJugadorId = @JugadorId
      AND ij.InscripcionEquipoId = @EquipoId
      AND ij.Estado = 'ACTIVO');
SELECT @@ROWCOUNT;";

            using (var conexion = ConexionBD.CrearConexion())
            using (var comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.Add("@PartidoId", SqlDbType.Int).Value = partidoId;
                comando.Parameters.Add("@EquipoId", SqlDbType.Int).Value = inscripcionEquipoId;
                comando.Parameters.Add("@JugadorId", SqlDbType.Int).Value = inscripcionJugadorId;
                comando.Parameters.Add("@Tipo", SqlDbType.VarChar, 15).Value = tipo;
                comando.Parameters.Add("@Minuto", SqlDbType.SmallInt).Value =
                    (object)minuto ?? DBNull.Value;
                comando.Parameters.Add("@Descripcion", SqlDbType.NVarChar, 500).Value =
                    String.IsNullOrWhiteSpace(descripcion)
                        ? (object)DBNull.Value
                        : descripcion.Trim();

                conexion.Open();
                int filas = Convert.ToInt32(comando.ExecuteScalar());
                if (filas != 1)
                {
                    throw new InvalidOperationException(
                        "El jugador no pertenece a uno de los equipos de ese partido.");
                }
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
