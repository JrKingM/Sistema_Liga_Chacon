using System;
using SistemaLigaChacon.Datos;
using SistemaLigaChacon.Logica;

namespace SistemaLigaChacon.Vistas
{
    public partial class Acceso : System.Web.UI.Page
    {
        protected void Page_Init(object sender, EventArgs e)
        {
            // Inicializa la sesion antes de proteger el ViewState. Asi el
            // primer postback usa la misma sesion que la carga inicial.
            Session["AccesoInicializado"] = true;
            ViewStateUserKey = Session.SessionID;
        }

        // Decide si se muestra el alta inicial o el formulario de inicio de sesion.
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UsuarioId"] != null)
            {
                string destino = SeguridadWeb.EsAdministrador(Session)
                    ? "~/Vistas/AdministrarPartidos.aspx"
                    : "~/Vistas/Home.aspx";
                Response.Redirect(ResolveUrl(destino), true);
                return;
            }

            try
            {
                bool hayUsuarios = UsuarioRepository.HayUsuarios();
                pnlConfiguracionInicial.Visible = !hayUsuarios;
                pnlLogin.Visible = hayUsuarios;
                if (!hayUsuarios && !Request.IsLocal)
                {
                    pnlConfiguracionInicial.Visible = false;
                    MostrarMensaje(
                        "Para crear la primera cuenta, abre esta pagina desde " +
                        "la computadora donde esta instalado el servidor.");
                }
            }
            catch (Exception)
            {
                MostrarMensaje("No se pudo consultar la base de datos. Verifica la conexion.");
                pnlConfiguracionInicial.Visible = false;
                pnlLogin.Visible = false;
            }
        }

        protected void btnCrearAdministrador_Click(object sender, EventArgs e)
        {
            // El primer administrador solo puede crearse en el equipo servidor.
            if (!Request.IsLocal)
            {
                MostrarMensaje("La configuracion inicial solo se permite desde la computadora del servidor.");
                return;
            }
            try
            {
                UsuarioRepository.CrearPrimerAdministrador(
                    txtNombreMostrar.Text,
                    txtNuevoUsuario.Text,
                    txtNuevaContrasena.Text);
                txtNuevaContrasena.Text = String.Empty;
                MostrarMensaje(
                    "Superadministrador creado. Inicia sesion con la cuenta registrada.",
                    false);
                pnlConfiguracionInicial.Visible = false;
                pnlLogin.Visible = true;
            }
            catch (ArgumentException ex) { MostrarMensaje(ex.Message); }
            catch (Exception) { MostrarMensaje("No se pudo crear la cuenta. Verifica los datos y vuelve a intentar."); }
        }

        protected void btnIngresar_Click(object sender, EventArgs e)
        {
            try
            {
                // La capa de datos valida el hash y devuelve el rol si coincide.
                CuentaUsuario cuenta = UsuarioRepository.Autenticar(txtUsuario.Text, txtContrasena.Text);
                txtContrasena.Text = String.Empty;
                if (cuenta == null)
                {
                    MostrarMensaje("Usuario o contrasena incorrectos.");
                    return;
                }

                Session.Clear();
                Session["UsuarioId"] = cuenta.UsuarioId;
                Session["NombreMostrar"] = cuenta.NombreMostrar;
                Session["Rol"] = cuenta.Rol;
                Session.Timeout = 30;
                string destino = cuenta.Rol == "Administrador"
                    ? "~/Vistas/AdministrarPartidos.aspx"
                    : "~/Vistas/Home.aspx";
                Response.Redirect(ResolveUrl(destino), false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (Exception)
            {
                MostrarMensaje(
                    "No se pudo iniciar sesion. Verifica la conexion con la base de datos.");
            }
        }

        private void MostrarMensaje(string mensaje, bool error = true)
        {
            lblMensaje.Text = mensaje;
            lblMensaje.CssClass = error ? "alert alert-danger d-block" : "alert alert-success d-block";
            lblMensaje.Visible = true;
        }
    }
}
