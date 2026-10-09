using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Security.Cryptography;
using SistemaLigaChacon.Logica;

namespace SistemaLigaChacon.Datos
{
    /// <summary>Gestiona cuentas, roles y verificacion segura de contrasenas.</summary>
    public static class UsuarioRepository
    {
        private const int Iteraciones = 120000;
        private const int SaltBytes = 16;
        private const int HashBytes = 32;

        public static bool HayUsuarios()
        {
            using (var cn = ConexionBD.CrearConexion())
            using (var cmd = new SqlCommand("SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.Usuarios) THEN 1 ELSE 0 END", cn))
            {
                cn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) == 1;
            }
        }

        /// <summary>Busca una cuenta activa y devuelve sus datos solo si la contrasena coincide.</summary>
        public static CuentaUsuario Autenticar(string nombreUsuario, string contrasena)
        {
            const string sql = @"SELECT TOP (1) UsuarioId, NombreMostrar, Rol, PasswordHash
FROM dbo.Usuarios WHERE NombreUsuario=@NombreUsuario AND Activo=1;";
            using (var cn = ConexionBD.CrearConexion())
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@NombreUsuario", SqlDbType.NVarChar, 60).Value = (nombreUsuario ?? String.Empty).Trim();
                cn.Open();
                using (var rd = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (!rd.Read() || !VerificarContrasena(contrasena, Convert.ToString(rd["PasswordHash"]))) return null;
                    string rol = Convert.ToString(rd["Rol"]);
                    return new CuentaUsuario
                    {
                        UsuarioId = Convert.ToInt32(rd["UsuarioId"]),
                        NombreMostrar = Convert.ToString(rd["NombreMostrar"]),
                        Rol = TraducirRol(rol)
                    };
                }
            }
        }

        /// <summary>Crea el superadministrador inicial si la tabla aun esta vacia.</summary>
        public static void CrearPrimerAdministrador(string nombreMostrar, string usuario, string contrasena)
        {
            CrearCuenta(
                nombreMostrar,
                usuario,
                contrasena,
                "SUPERADMINISTRADOR",
                true);
        }

        /// <summary>Crea una cuenta de administrador o de usuario de consulta.</summary>
        public static void CrearUsuario(
            string nombreMostrar,
            string usuario,
            string contrasena,
            string rol)
        {
            if (rol != "ADMINISTRADOR" && rol != "USUARIO")
                throw new ArgumentException("Rol de cuenta no permitido.");

            CrearCuenta(nombreMostrar, usuario, contrasena, rol, false);
        }

        public static DataTable ObtenerUsuarios()
        {
            const string sql = @"SELECT UsuarioId, NombreUsuario, NombreMostrar,
CASE Rol
    WHEN 'SUPERADMINISTRADOR' THEN N'Superadministrador'
    WHEN 'ADMINISTRADOR' THEN N'Administrador'
    ELSE N'Usuario'
END AS Rol,
Activo, CreadoEn FROM dbo.Usuarios ORDER BY NombreMostrar;";
            var tabla = new DataTable();
            using (var cn = ConexionBD.CrearConexion())
            using (var da = new SqlDataAdapter(sql, cn)) da.Fill(tabla);
            return tabla;
        }

        private static void CrearCuenta(
            string nombreMostrar,
            string usuario,
            string contrasena,
            string rol,
            bool soloSiVacia)
        {
            ValidarCuenta(nombreMostrar, usuario, contrasena);
            string hash = CrearHash(contrasena);
            using (var cn = ConexionBD.CrearConexion())
            using (var cmd = new SqlCommand(@"
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF @SoloSiVacia=1 AND EXISTS (SELECT 1 FROM dbo.Usuarios WITH (TABLOCKX, HOLDLOCK))
BEGIN ROLLBACK TRANSACTION; THROW 50002, 'La configuracion inicial ya fue completada.', 1; END;
IF EXISTS (SELECT 1 FROM dbo.Usuarios WHERE NombreUsuario=@Usuario)
BEGIN ROLLBACK TRANSACTION; THROW 50003, 'Ese nombre de usuario ya existe.', 1; END;
INSERT dbo.Usuarios (NombreUsuario, NombreMostrar, PasswordHash, Rol, Activo)
VALUES (@Usuario, @NombreMostrar, @PasswordHash, @Rol, 1);
COMMIT TRANSACTION;", cn))
            {
                cmd.Parameters.Add("@SoloSiVacia", SqlDbType.Bit).Value = soloSiVacia;
                cmd.Parameters.Add("@Usuario", SqlDbType.NVarChar, 60).Value = usuario.Trim();
                cmd.Parameters.Add("@NombreMostrar", SqlDbType.NVarChar, 120).Value = nombreMostrar.Trim();
                cmd.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 255).Value = hash;
                cmd.Parameters.Add("@Rol", SqlDbType.VarChar, 25).Value = rol;
                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private static string TraducirRol(string rol)
        {
            if (rol == "SUPERADMINISTRADOR") return "Superadministrador";
            if (rol == "ADMINISTRADOR") return "Administrador";
            return "Usuario";
        }

        private static void ValidarCuenta(string nombreMostrar, string usuario, string contrasena)
        {
            if (String.IsNullOrWhiteSpace(nombreMostrar) || nombreMostrar.Trim().Length > 120)
                throw new ArgumentException("Escribe un nombre para mostrar (maximo 120 caracteres).");
            if (String.IsNullOrWhiteSpace(usuario) || usuario.Trim().Length > 60)
                throw new ArgumentException("El usuario debe tener entre 1 y 60 caracteres.");
            if (String.IsNullOrEmpty(contrasena) || contrasena.Length < 10 || contrasena.Length > 200)
                throw new ArgumentException("La contrasena debe tener entre 10 y 200 caracteres.");
        }

        /// <summary>Deriva un hash PBKDF2 con una sal nueva para guardar, nunca la clave original.</summary>
        private static string CrearHash(string contrasena)
        {
            byte[] salt = new byte[SaltBytes];
            using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(salt);
            byte[] hash;
            using (var derivador = new Rfc2898DeriveBytes(contrasena, salt, Iteraciones, HashAlgorithmName.SHA256))
                hash = derivador.GetBytes(HashBytes);
            return "PBKDF2-SHA256$" + Iteraciones.ToString(CultureInfo.InvariantCulture) + "$" +
                   Convert.ToBase64String(salt) + "$" + Convert.ToBase64String(hash);
        }

        /// <summary>Compara el hash derivado de la clave ingresada con el hash almacenado.</summary>
        private static bool VerificarContrasena(string contrasena, string almacenada)
        {
            if (String.IsNullOrEmpty(contrasena) || String.IsNullOrEmpty(almacenada)) return false;
            string[] partes = almacenada.Split('$');
            int iteraciones;
            if (partes.Length != 4 || partes[0] != "PBKDF2-SHA256" ||
                !Int32.TryParse(partes[1], NumberStyles.None, CultureInfo.InvariantCulture, out iteraciones) || iteraciones < 10000)
                return false;
            try
            {
                byte[] salt = Convert.FromBase64String(partes[2]);
                byte[] hashEsperado = Convert.FromBase64String(partes[3]);
                if (salt.Length < 16 || hashEsperado.Length != HashBytes) return false;
                byte[] calculado;
                using (var derivador = new Rfc2898DeriveBytes(contrasena, salt, iteraciones, HashAlgorithmName.SHA256))
                    calculado = derivador.GetBytes(hashEsperado.Length);
                int diferencia = 0;
                for (int i = 0; i < hashEsperado.Length; i++) diferencia |= hashEsperado[i] ^ calculado[i];
                return diferencia == 0;
            }
            catch (FormatException) { return false; }
        }
    }
}
