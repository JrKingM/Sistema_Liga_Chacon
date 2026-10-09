using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SistemaLigaChacon.Vistas
{
    public partial class Plantilla : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            bool haySesion = Session["UsuarioId"] != null;
            lnkAcceso.Visible = !haySesion;
            lnkAcceso.NavigateUrl = ResolveUrl("~/Vistas/Acceso.aspx");
            btnCerrarSesion.Visible = haySesion;
            if (haySesion)
            {
                litUsuarioActual.Text = "<span class=\"nav-link text-secondary small\">" +
                    Server.HtmlEncode(Convert.ToString(Session["NombreMostrar"])) + "</span>";
            }
            bool esAdmin = SistemaLigaChacon.Logica.SeguridadWeb.EsAdministrador(Session);
            phAdminMenu.Visible = esAdmin;
            lnkAdminUsuarios.Visible =
                SistemaLigaChacon.Logica.SeguridadWeb.EsSuperadministrador(Session);
        }

        protected void btnCerrarSesion_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect(ResolveUrl("~/Vistas/Home.aspx"), true);
        }
    }
}
