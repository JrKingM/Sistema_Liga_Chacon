using System;
using System.Data;
using System.Data.SqlClient;

namespace SistemaLigaChacon.Datos
{
    /// <summary>Operaciones de administracion de competencias, jornadas y partidos.</summary>
    public static class AdministracionRepository
    {
        /// <summary>Lista las competencias disponibles para programar partidos.</summary>
        public static DataTable ObtenerCompetencias()
        {
            const string sql = @"SELECT c.CompetenciaId, ca.Nombre + N' - ' + t.Nombre AS Descripcion
FROM dbo.Competencias c
JOIN dbo.Categorias ca ON ca.CategoriaId=c.CategoriaId
JOIN dbo.Temporadas t ON t.TemporadaId=c.TemporadaId
ORDER BY t.TemporadaId DESC, ca.Nombre;";
            return Consultar(sql, null);
        }

        /// <summary>Crea las jornadas 1 a 9 si aun no existen para la competencia.</summary>
        public static void AsegurarJornadas(int competenciaId)
        {
            const string sql = @"INSERT dbo.Jornadas (CompetenciaId, Numero)
SELECT @CompetenciaId, n.Numero
FROM (VALUES (1),(2),(3),(4),(5),(6),(7),(8),(9)) n(Numero)
WHERE EXISTS (SELECT 1 FROM dbo.Competencias WHERE CompetenciaId=@CompetenciaId)
  AND NOT EXISTS (SELECT 1 FROM dbo.Jornadas j WHERE j.CompetenciaId=@CompetenciaId AND j.Numero=n.Numero);";
            Ejecutar(sql, cmd => cmd.Parameters.Add("@CompetenciaId", SqlDbType.Int).Value = competenciaId);
        }

        public static DataTable ObtenerJornadas(int competenciaId)
        {
            const string sql = @"
SELECT JornadaId, Numero
FROM dbo.Jornadas
WHERE CompetenciaId = @CompetenciaId
ORDER BY Numero;";
            return Consultar(sql, cmd => cmd.Parameters.Add("@CompetenciaId", SqlDbType.Int).Value = competenciaId);
        }

        public static DataTable ObtenerEquiposCompetencia(int competenciaId)
        {
            const string sql = @"SELECT ei.InscripcionEquipoId, e.Nombre
FROM dbo.EquipoInscripciones ei JOIN dbo.Equipos e ON e.EquipoId=ei.EquipoId
WHERE ei.CompetenciaId=@CompetenciaId AND ei.Estado='ACTIVO' AND e.Estado='ACTIVO'
ORDER BY e.Nombre;";
            return Consultar(sql, cmd => cmd.Parameters.Add("@CompetenciaId", SqlDbType.Int).Value = competenciaId);
        }

        /// <summary>Guarda un partido pendiente despues de validar cancha y equipos inscritos.</summary>
        public static int CrearPartido(
            int competenciaId,
            int jornadaNumero,
            int equipoLocalId,
            int equipoVisitaId,
            DateTime fechaHora,
            string cancha)
        {
            if (String.IsNullOrWhiteSpace(cancha))
                throw new ArgumentException("La cancha es obligatoria.", "cancha");

            const string sql = @"
INSERT dbo.Partidos (
    CompetenciaId,
    JornadaId,
    EquipoLocalInscripcionId,
    EquipoVisitaInscripcionId,
    FechaHora,
    Cancha,
    Estado)
SELECT @CompetenciaId, j.JornadaId, @EquipoLocal, @EquipoVisita, @FechaHora, @Cancha, 'PENDIENTE'
FROM dbo.Jornadas j
WHERE j.CompetenciaId=@CompetenciaId AND j.Numero=@Jornada
  AND @EquipoLocal<>@EquipoVisita
  AND EXISTS (
      SELECT 1
      FROM dbo.EquipoInscripciones
      WHERE InscripcionEquipoId=@EquipoLocal
        AND CompetenciaId=@CompetenciaId
        AND Estado='ACTIVO')
  AND EXISTS (
      SELECT 1
      FROM dbo.EquipoInscripciones
      WHERE InscripcionEquipoId=@EquipoVisita
        AND CompetenciaId=@CompetenciaId
        AND Estado='ACTIVO');
SELECT CAST(SCOPE_IDENTITY() AS INT);";
            using (var cn = ConexionBD.CrearConexion())
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@CompetenciaId", SqlDbType.Int).Value = competenciaId;
                cmd.Parameters.Add("@Jornada", SqlDbType.SmallInt).Value = jornadaNumero;
                cmd.Parameters.Add("@EquipoLocal", SqlDbType.Int).Value = equipoLocalId;
                cmd.Parameters.Add("@EquipoVisita", SqlDbType.Int).Value = equipoVisitaId;
                cmd.Parameters.Add("@FechaHora", SqlDbType.DateTime2).Value = fechaHora;
                cmd.Parameters.Add("@Cancha", SqlDbType.NVarChar, 120).Value = cancha.Trim();
                cn.Open();
                object id = cmd.ExecuteScalar();
                if (id == null || id == DBNull.Value)
                {
                    throw new InvalidOperationException(
                        "No se pudo crear el partido. Revisa la jornada y " +
                        "los equipos seleccionados.");
                }
                return Convert.ToInt32(id);
            }
        }

        public static DataTable ObtenerPartidosCompetencia(int competenciaId)
        {
            const string sql = @"SELECT p.PartidoId, p.VersionFila, j.Numero AS Jornada, p.FechaHora, p.Cancha,
el.Nombre AS EquipoLocal, ev.Nombre AS EquipoVisita, p.GolesLocal, p.GolesVisita, p.Estado
FROM dbo.Partidos p
JOIN dbo.Jornadas j ON j.JornadaId=p.JornadaId
JOIN dbo.EquipoInscripciones eil ON eil.InscripcionEquipoId=p.EquipoLocalInscripcionId
JOIN dbo.Equipos el ON el.EquipoId=eil.EquipoId
JOIN dbo.EquipoInscripciones eiv ON eiv.InscripcionEquipoId=p.EquipoVisitaInscripcionId
JOIN dbo.Equipos ev ON ev.EquipoId=eiv.EquipoId
WHERE p.CompetenciaId=@CompetenciaId AND p.Estado<>'CANCELADO'
ORDER BY p.FechaHora DESC, j.Numero, p.PartidoId DESC;";
            return Consultar(sql, cmd => cmd.Parameters.Add("@CompetenciaId", SqlDbType.Int).Value = competenciaId);
        }

        public static DataTable ObtenerPartidoParaEdicion(int partidoId, int competenciaId)
        {
            const string sql = @"SELECT p.PartidoId, p.VersionFila, j.Numero AS Jornada, p.FechaHora, p.Cancha,
p.EquipoLocalInscripcionId, p.EquipoVisitaInscripcionId, p.Estado
FROM dbo.Partidos p JOIN dbo.Jornadas j ON j.JornadaId=p.JornadaId
WHERE p.PartidoId=@PartidoId AND p.CompetenciaId=@CompetenciaId;";
            using (var cn = ConexionBD.CrearConexion())
            using (var cmd = new SqlCommand(sql, cn))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.Parameters.Add("@PartidoId", SqlDbType.Int).Value = partidoId;
                cmd.Parameters.Add("@CompetenciaId", SqlDbType.Int).Value = competenciaId;
                var tabla = new DataTable();
                da.Fill(tabla);
                return tabla;
            }
        }

        /// <summary>Actualiza solo una programacion pendiente que conserve la version cargada.</summary>
        public static bool ActualizarProgramacion(int partidoId, int competenciaId, int jornadaNumero,
            int equipoLocalId, int equipoVisitaId, DateTime fechaHora, string cancha, byte[] versionFila)
        {
            if (String.IsNullOrWhiteSpace(cancha)) throw new ArgumentException("La cancha es obligatoria.", "cancha");
            const string sql = @"UPDATE p SET JornadaId=j.JornadaId,
    EquipoLocalInscripcionId=@EquipoLocal, EquipoVisitaInscripcionId=@EquipoVisita,
    FechaHora=@FechaHora, Cancha=@Cancha
FROM dbo.Partidos p
JOIN dbo.Jornadas j ON j.CompetenciaId=p.CompetenciaId AND j.Numero=@Jornada
WHERE p.PartidoId=@PartidoId AND p.CompetenciaId=@CompetenciaId
  AND p.Estado='PENDIENTE' AND p.VersionFila=@VersionFila
  AND @EquipoLocal<>@EquipoVisita
  AND EXISTS (
      SELECT 1
      FROM dbo.EquipoInscripciones
      WHERE InscripcionEquipoId=@EquipoLocal
        AND CompetenciaId=@CompetenciaId
        AND Estado='ACTIVO')
  AND EXISTS (
      SELECT 1
      FROM dbo.EquipoInscripciones
      WHERE InscripcionEquipoId=@EquipoVisita
        AND CompetenciaId=@CompetenciaId
        AND Estado='ACTIVO');";
            using (var cn = ConexionBD.CrearConexion())
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@PartidoId", SqlDbType.Int).Value = partidoId;
                cmd.Parameters.Add("@CompetenciaId", SqlDbType.Int).Value = competenciaId;
                cmd.Parameters.Add("@Jornada", SqlDbType.SmallInt).Value = jornadaNumero;
                cmd.Parameters.Add("@EquipoLocal", SqlDbType.Int).Value = equipoLocalId;
                cmd.Parameters.Add("@EquipoVisita", SqlDbType.Int).Value = equipoVisitaId;
                cmd.Parameters.Add("@FechaHora", SqlDbType.DateTime2).Value = fechaHora;
                cmd.Parameters.Add("@Cancha", SqlDbType.NVarChar, 120).Value = cancha.Trim();
                cmd.Parameters.Add("@VersionFila", SqlDbType.Binary, 8).Value = versionFila;
                cn.Open();
                return cmd.ExecuteNonQuery() == 1;
            }
        }

        /// <summary>Elimina solo partidos pendientes y evita borrar una fila que ya cambio.</summary>
        public static bool EliminarPartidoPendiente(int partidoId, byte[] versionFila)
        {
            const string sql = @"DELETE dbo.Partidos
WHERE PartidoId=@PartidoId AND Estado='PENDIENTE' AND VersionFila=@VersionFila;";
            using (var cn = ConexionBD.CrearConexion())
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@PartidoId", SqlDbType.Int).Value = partidoId;
                cmd.Parameters.Add("@VersionFila", SqlDbType.Binary, 8).Value = versionFila;
                cn.Open();
                return cmd.ExecuteNonQuery() == 1;
            }
        }

        /// <summary>Guarda el marcador y cambia el estado del partido a FINALIZADO.</summary>
        public static bool GuardarResultado(int partidoId, int golesLocal, int golesVisita, byte[] versionFila)
        {
            const string sql = @"
UPDATE dbo.Partidos
SET GolesLocal=@GolesLocal,
    GolesVisita=@GolesVisita,
    Estado='FINALIZADO'
WHERE PartidoId=@PartidoId AND VersionFila=@VersionFila AND Estado<>'CANCELADO';";
            using (var cn = ConexionBD.CrearConexion())
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@GolesLocal", SqlDbType.SmallInt).Value = golesLocal;
                cmd.Parameters.Add("@GolesVisita", SqlDbType.SmallInt).Value = golesVisita;
                cmd.Parameters.Add("@PartidoId", SqlDbType.Int).Value = partidoId;
                cmd.Parameters.Add("@VersionFila", SqlDbType.Binary, 8).Value = versionFila;
                cn.Open();
                return cmd.ExecuteNonQuery() == 1;
            }
        }

        private static DataTable Consultar(string sql, Action<SqlCommand> parametros)
        {
            var tabla = new DataTable();
            using (var cn = ConexionBD.CrearConexion())
            using (var cmd = new SqlCommand(sql, cn))
            using (var da = new SqlDataAdapter(cmd))
            {
                if (parametros != null) parametros(cmd);
                da.Fill(tabla);
            }
            return tabla;
        }

        private static void Ejecutar(string sql, Action<SqlCommand> parametros)
        {
            using (var cn = ConexionBD.CrearConexion())
            using (var cmd = new SqlCommand(sql, cn))
            {
                parametros(cmd);
                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}
