using System;
using System.Web.SessionState;
using System.Web.UI;

namespace SistemaLigaChacon.Logica
{
    public static class SeguridadWeb
    {
        // Identifica al superadministrador, que puede administrar cuentas.
        public static bool EsSuperadministrador(HttpSessionState sesion)
        {
            return sesion != null && String.Equals(
                Convert.ToString(sesion["Rol"]),
                "Superadministrador",
                StringComparison.OrdinalIgnoreCase);
        }

        // Tanto el superadministrador como el administrador operan el torneo.
        public static bool EsAdministrador(HttpSessionState sesion)
        {
            return EsSuperadministrador(sesion) ||
                (sesion != null && String.Equals(
                    Convert.ToString(sesion["Rol"]),
                    "Administrador",
                    StringComparison.OrdinalIgnoreCase));
        }

        // Usar este control al entrar a cualquier pagina administrativa.
        public static bool RequerirAdministrador(Page pagina)
        {
            if (EsAdministrador(pagina.Session)) return true;
            pagina.Response.Redirect(pagina.ResolveUrl("~/Vistas/Acceso.aspx"), true);
            return false;
        }

        // Restringe las funciones de cuentas al rol de mayor privilegio.
        public static bool RequerirSuperadministrador(Page pagina)
        {
            if (EsSuperadministrador(pagina.Session)) return true;
            pagina.Response.Redirect(
                pagina.ResolveUrl("~/Vistas/Acceso.aspx"),
                true);
            return false;
        }

        // Permite ampliar paginas de solo lectura para usuarios autenticados.
        public static bool RequerirSesion(Page pagina)
        {
            if (pagina.Session["UsuarioId"] != null) return true;
            pagina.Response.Redirect(pagina.ResolveUrl("~/Vistas/Acceso.aspx"), true);
            return false;
        }
    }
}
