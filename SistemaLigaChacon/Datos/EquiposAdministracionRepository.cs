using System;
using System.Data;
using System.Data.SqlClient;

namespace SistemaLigaChacon.Datos
{
    /// <summary>Registra clubes y los inscribe en la competencia activa.</summary>
    public static class EquiposAdministracionRepository
    {
        public static DataTable ObtenerCategoriasActivas()
        {
            const string sql = @"
SELECT c.CategoriaId,
       ca.Nombre + N' - ' + t.Nombre AS Descripcion
FROM dbo.Competencias c
INNER JOIN dbo.Categorias ca ON ca.CategoriaId = c.CategoriaId
INNER JOIN dbo.Temporadas t ON t.TemporadaId = c.TemporadaId
WHERE c.Estado <> 'FINALIZADA'
  AND t.Estado <> 'FINALIZADA'
ORDER BY t.TemporadaId DESC, ca.Nombre;";

            return Consultar(sql, null);
        }

        public static DataTable ObtenerEquipos(int categoriaId)
        {
            const string sql = @"
SELECT e.Codigo, e.Nombre, e.Representante, e.Telefono,
       e.Estado, c.Nombre AS Categoria
FROM dbo.Equipos e
INNER JOIN dbo.EquipoInscripciones ei ON ei.EquipoId = e.EquipoId
INNER JOIN dbo.Competencias co ON co.CompetenciaId = ei.CompetenciaId
INNER JOIN dbo.Categorias c ON c.CategoriaId = co.CategoriaId
WHERE co.CategoriaId = @CategoriaId
  AND ei.Estado = 'ACTIVO'
ORDER BY e.Nombre;";

            return Consultar(sql, comando =>
                comando.Parameters.Add("@CategoriaId", SqlDbType.Int)
                    .Value = categoriaId);
        }

        public static DataTable ObtenerInscripcionesEquipo(int categoriaId)
        {
            const string sql = @"
SELECT ei.InscripcionEquipoId, e.Nombre
FROM dbo.EquipoInscripciones ei
INNER JOIN dbo.Equipos e ON e.EquipoId = ei.EquipoId
INNER JOIN dbo.Competencias co ON co.CompetenciaId = ei.CompetenciaId
WHERE co.CategoriaId = @CategoriaId
  AND ei.Estado = 'ACTIVO'
  AND e.Estado = 'ACTIVO'
ORDER BY e.Nombre;";

            return Consultar(sql, comando =>
                comando.Parameters.Add("@CategoriaId", SqlDbType.Int)
                    .Value = categoriaId);
        }

        public static DataTable ObtenerPlantilla(int inscripcionEquipoId)
        {
            const string sql = @"
SELECT j.Nombres, j.Apellidos, ij.Dorsal
FROM dbo.InscripcionesJugadores ij
INNER JOIN dbo.Jugadores j ON j.JugadorId = ij.JugadorId
WHERE ij.InscripcionEquipoId = @InscripcionEquipoId
  AND ij.Estado = 'ACTIVO'
  AND j.Estado = 'ACTIVO'
ORDER BY ij.Dorsal, j.Apellidos, j.Nombres;";

            return Consultar(sql, comando =>
                comando.Parameters.Add("@InscripcionEquipoId", SqlDbType.Int)
                    .Value = inscripcionEquipoId);
        }

        public static void AgregarJugador(
            string nombres,
            string apellidos,
            short? dorsal,
            int inscripcionEquipoId)
        {
            if (String.IsNullOrWhiteSpace(nombres) || nombres.Trim().Length > 80)
                throw new ArgumentException("Escribe los nombres del jugador.");
            if (String.IsNullOrWhiteSpace(apellidos) || apellidos.Trim().Length > 100)
                throw new ArgumentException("Escribe los apellidos del jugador.");

            const string sql = @"
SET XACT_ABORT ON;
BEGIN TRANSACTION;

INSERT dbo.Jugadores (Nombres, Apellidos)
VALUES (@Nombres, @Apellidos);

DECLARE @JugadorId INT = CONVERT(INT, SCOPE_IDENTITY());

INSERT dbo.InscripcionesJugadores
    (InscripcionEquipoId, JugadorId, Dorsal)
SELECT @InscripcionEquipoId, @JugadorId, @Dorsal
WHERE EXISTS (
    SELECT 1
    FROM dbo.EquipoInscripciones ei
    INNER JOIN dbo.Equipos e ON e.EquipoId = ei.EquipoId
    WHERE ei.InscripcionEquipoId = @InscripcionEquipoId
      AND ei.Estado = 'ACTIVO'
      AND e.Estado = 'ACTIVO');

IF @@ROWCOUNT <> 1
BEGIN
    ROLLBACK TRANSACTION;
    THROW 50012, 'El equipo ya no esta activo en la competencia.', 1;
END;

COMMIT TRANSACTION;";

            using (var conexion = ConexionBD.CrearConexion())
            using (var comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.Add("@Nombres", SqlDbType.NVarChar, 80)
                    .Value = nombres.Trim();
                comando.Parameters.Add("@Apellidos", SqlDbType.NVarChar, 100)
                    .Value = apellidos.Trim();
                comando.Parameters.Add("@Dorsal", SqlDbType.SmallInt).Value =
                    (object)dorsal ?? DBNull.Value;
                comando.Parameters.Add("@InscripcionEquipoId", SqlDbType.Int)
                    .Value = inscripcionEquipoId;

                conexion.Open();
                comando.ExecuteNonQuery();
            }
        }

        public static void CrearEquipo(
            string codigo,
            string nombre,
            string representante,
            string telefono,
            string logoUrl,
            int categoriaId)
        {
            if (String.IsNullOrWhiteSpace(codigo) || codigo.Trim().Length > 10)
                throw new ArgumentException("El codigo debe tener entre 1 y 10 caracteres.");
            if (String.IsNullOrWhiteSpace(nombre) || nombre.Trim().Length > 120)
                throw new ArgumentException("El nombre del equipo es obligatorio.");

            const string sql = @"
SET XACT_ABORT ON;
BEGIN TRANSACTION;

DECLARE @CompetenciaId INT;
SELECT TOP (1) @CompetenciaId = co.CompetenciaId
FROM dbo.Competencias co WITH (UPDLOCK, HOLDLOCK)
INNER JOIN dbo.Temporadas t ON t.TemporadaId = co.TemporadaId
WHERE co.CategoriaId = @CategoriaId
  AND co.Estado <> 'FINALIZADA'
  AND t.Estado <> 'FINALIZADA'
ORDER BY t.TemporadaId DESC;

IF @CompetenciaId IS NULL
BEGIN
    ROLLBACK TRANSACTION;
    THROW 50010, 'No existe una competencia activa para esa categoria.', 1;
END;

IF (SELECT COUNT(*)
    FROM dbo.EquipoInscripciones WITH (UPDLOCK, HOLDLOCK)
    WHERE CompetenciaId = @CompetenciaId
      AND Estado = 'ACTIVO') >= 12
BEGIN
    ROLLBACK TRANSACTION;
    THROW 50011, 'La competencia ya tiene el maximo de 12 equipos.', 1;
END;

INSERT dbo.Equipos (Codigo, Nombre, Representante, Telefono, LogoUrl)
VALUES (@Codigo, @Nombre, @Representante, @Telefono, @LogoUrl);

DECLARE @EquipoId INT = CONVERT(INT, SCOPE_IDENTITY());

INSERT dbo.EquipoInscripciones (CompetenciaId, EquipoId, Estado)
VALUES (@CompetenciaId, @EquipoId, 'ACTIVO');

COMMIT TRANSACTION;";

            using (var conexion = ConexionBD.CrearConexion())
            using (var comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.Add("@Codigo", SqlDbType.VarChar, 10)
                    .Value = codigo.Trim().ToUpperInvariant();
                comando.Parameters.Add("@Nombre", SqlDbType.NVarChar, 120)
                    .Value = nombre.Trim();
                comando.Parameters.Add("@Representante", SqlDbType.NVarChar, 120)
                    .Value = TextoONull(representante);
                comando.Parameters.Add("@Telefono", SqlDbType.NVarChar, 30)
                    .Value = TextoONull(telefono);
                comando.Parameters.Add("@LogoUrl", SqlDbType.NVarChar, 260)
                    .Value = TextoONull(logoUrl);
                comando.Parameters.Add("@CategoriaId", SqlDbType.Int)
                    .Value = categoriaId;

                conexion.Open();
                comando.ExecuteNonQuery();
            }
        }

        private static object TextoONull(string valor)
        {
            return String.IsNullOrWhiteSpace(valor)
                ? (object)DBNull.Value
                : valor.Trim();
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
