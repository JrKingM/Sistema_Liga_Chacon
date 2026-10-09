using System;
using System.Configuration;
using System.Data.SqlClient;

namespace SistemaLigaChacon.Datos
{
    /// <summary>
    /// Punto unico para abrir conexiones a la base LigaArnaldoChacon.
    /// La instancia y autenticacion se configuran en Web.config.
    /// </summary>
    public static class ConexionBD
    {
        public static SqlConnection CrearConexion()
        {
            ConnectionStringSettings configuracion = 
                ConfigurationManager.ConnectionStrings["LigaArnaldoChacon"];

            if (configuracion == null || String.IsNullOrWhiteSpace(configuracion.ConnectionString))
            {
                throw new InvalidOperationException(
                    "Falta configurar la cadena LigaArnaldoChacon en Web.config.");
            }

            return new SqlConnection(configuracion.ConnectionString);
        }
    }
}
