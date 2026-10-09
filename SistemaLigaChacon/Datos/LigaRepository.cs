using System;
using System.Data;
using System.Data.SqlClient;

namespace SistemaLigaChacon.Datos
{
    /// <summary>Consultas de solo lectura para las vistas publicas de la liga.</summary>
    public static class LigaRepository
    {
        /// <summary>Obtiene equipos activos y su categoria activa mas reciente.</summary>
        public static DataTable ObtenerEquipos()
        {
            const string sql = @"
SELECT e.EquipoId, e.Codigo, e.Nombre, e.LogoUrl, e.Estado,
       COALESCE(categoria.Nombre, N'Categoria por asignar') AS Categoria
FROM dbo.Equipos e
OUTER APPLY
(
    SELECT TOP (1) ca.Nombre
    FROM dbo.EquipoInscripciones ei
    INNER JOIN dbo.Competencias c ON c.CompetenciaId = ei.CompetenciaId
    INNER JOIN dbo.Temporadas t ON t.TemporadaId = c.TemporadaId
    INNER JOIN dbo.Categorias ca ON ca.CategoriaId = c.CategoriaId
    WHERE ei.EquipoId = e.EquipoId AND ei.Estado = 'ACTIVO'
    ORDER BY t.TemporadaId DESC, c.CompetenciaId DESC
) categoria
WHERE e.Estado = 'ACTIVO'
ORDER BY e.Nombre;";

            return Consultar(sql, null);
        }

        /// <summary>Obtiene solo los equipos activos inscritos en la categoria solicitada.</summary>
        public static DataTable ObtenerEquipos(string categoriaSolicitada)
        {
            const string sql = @"
SELECT e.EquipoId, e.Codigo, e.Nombre, e.LogoUrl, e.Estado,
       categoria.Nombre AS Categoria
FROM dbo.Equipos e
CROSS APPLY
(
    SELECT TOP (1) ca.Nombre
    FROM dbo.EquipoInscripciones ei
    INNER JOIN dbo.Competencias c ON c.CompetenciaId = ei.CompetenciaId
    INNER JOIN dbo.Temporadas t ON t.TemporadaId = c.TemporadaId
    INNER JOIN dbo.Categorias ca ON ca.CategoriaId = c.CategoriaId
    WHERE ei.EquipoId = e.EquipoId AND ei.Estado = 'ACTIVO'
    ORDER BY t.TemporadaId DESC, c.CompetenciaId DESC
) categoria
WHERE e.Estado = 'ACTIVO' AND categoria.Nombre = @Categoria
ORDER BY e.Nombre;";

            return Consultar(sql, cmd =>
                cmd.Parameters.Add("@Categoria", SqlDbType.NVarChar, 30).Value = categoriaSolicitada);
        }

        /// <summary>Lee la vista SQL de posiciones para la ultima temporada y categoria solicitada.</summary>
        public static DataTable ObtenerTablaPosiciones(string categoria)
        {
            const string sql = @"
SELECT p.Equipo, p.LogoUrl, p.PJ, p.PG, p.PE, p.PP, p.GF, p.GC, p.DG, p.PTS
FROM dbo.vw_TablaPosiciones p
INNER JOIN dbo.Competencias c ON c.CompetenciaId = p.CompetenciaId
INNER JOIN dbo.Temporadas t ON t.TemporadaId = c.TemporadaId
WHERE p.Categoria = @Categoria
  AND t.TemporadaId = (SELECT MAX(t2.TemporadaId) FROM dbo.Temporadas t2)
ORDER BY p.PTS DESC, p.DG DESC, p.GF DESC;";

            return Consultar(sql, cmd => cmd.Parameters.Add("@Categoria", SqlDbType.NVarChar, 30).Value = categoria);
        }

        /// <summary>Lista partidos aun no finalizados, priorizando fechas futuras.</summary>
        public static DataTable ObtenerPartidosProximos(int limite, string categoria)
        {
            const string sql = @"
SELECT TOP (@Limite) j.Numero AS Jornada, c.Nombre AS Categoria, p.FechaHora, p.Cancha,
       eqLocal.Nombre AS EquipoLocal, eqVisita.Nombre AS EquipoVisita
FROM dbo.Partidos p
INNER JOIN dbo.Jornadas j ON j.JornadaId = p.JornadaId
INNER JOIN dbo.Competencias co ON co.CompetenciaId = p.CompetenciaId
INNER JOIN dbo.Categorias c ON c.CategoriaId = co.CategoriaId
INNER JOIN dbo.EquipoInscripciones eil ON eil.InscripcionEquipoId = p.EquipoLocalInscripcionId
INNER JOIN dbo.Equipos eqLocal ON eqLocal.EquipoId = eil.EquipoId
INNER JOIN dbo.EquipoInscripciones eiv ON eiv.InscripcionEquipoId = p.EquipoVisitaInscripcionId
INNER JOIN dbo.Equipos eqVisita ON eqVisita.EquipoId = eiv.EquipoId
WHERE p.Estado IN ('PENDIENTE', 'EN_CURSO')
  AND c.Nombre = @Categoria
ORDER BY CASE WHEN p.FechaHora >= GETDATE() THEN 0 ELSE 1 END,
         p.FechaHora ASC, p.PartidoId ASC;";

            return Consultar(sql, cmd =>
            {
                cmd.Parameters.Add("@Limite", SqlDbType.Int).Value = limite;
                cmd.Parameters.Add("@Categoria", SqlDbType.NVarChar, 30).Value = categoria;
            });
        }

        /// <summary>Lista marcadores finalizados mas recientes de la categoria solicitada.</summary>
        public static DataTable ObtenerUltimosResultados(int limite, string categoria)
        {
            const string sql = @"
SELECT TOP (@Limite) j.Numero AS Jornada, c.Nombre AS Categoria, p.FechaHora,
       eqLocal.Nombre AS EquipoLocal, eqVisita.Nombre AS EquipoVisita,
       p.GolesLocal, p.GolesVisita
FROM dbo.Partidos p
INNER JOIN dbo.Jornadas j ON j.JornadaId = p.JornadaId
INNER JOIN dbo.Competencias co ON co.CompetenciaId = p.CompetenciaId
INNER JOIN dbo.Categorias c ON c.CategoriaId = co.CategoriaId
INNER JOIN dbo.EquipoInscripciones eil ON eil.InscripcionEquipoId = p.EquipoLocalInscripcionId
INNER JOIN dbo.Equipos eqLocal ON eqLocal.EquipoId = eil.EquipoId
INNER JOIN dbo.EquipoInscripciones eiv ON eiv.InscripcionEquipoId = p.EquipoVisitaInscripcionId
INNER JOIN dbo.Equipos eqVisita ON eqVisita.EquipoId = eiv.EquipoId
WHERE p.Estado = 'FINALIZADO'
  AND c.Nombre = @Categoria
ORDER BY p.FechaHora DESC, p.PartidoId DESC;";

            return Consultar(sql, cmd =>
            {
                cmd.Parameters.Add("@Limite", SqlDbType.Int).Value = limite;
                cmd.Parameters.Add("@Categoria", SqlDbType.NVarChar, 30).Value = categoria;
            });
        }

        private static DataTable Consultar(string sql, Action<SqlCommand> agregarParametros)
        {
            var tabla = new DataTable();
            using (var conexion = ConexionBD.CrearConexion())
            using (var comando = new SqlCommand(sql, conexion))
            using (var adaptador = new SqlDataAdapter(comando))
            {
                if (agregarParametros != null) agregarParametros(comando);
                adaptador.Fill(tabla);
            }
            return tabla;
        }
    }
}
