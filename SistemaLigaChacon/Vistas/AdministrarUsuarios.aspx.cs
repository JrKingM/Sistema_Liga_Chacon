using System;
using SistemaLigaChacon.Datos;
using SistemaLigaChacon.Logica;

namespace SistemaLigaChacon.Vistas
{
    public partial class AdministrarUsuarios : System.Web.UI.Page
    {
        // La clave de ViewState se liga a la sesion actual para evitar reuso.
        protected void Page_Init(object sender, EventArgs e) { ViewStateUserKey = Session.SessionID; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!SeguridadWeb.RequerirSuperadministrador(this)) return;
            if (!IsPostBack) CargarUsuarios();
        }

        protected void btnCrear_Click(object sender, EventArgs e)
        {
            // Se comprueba el permiso otra vez al guardar, no solo al cargar.
            if (!SeguridadWeb.RequerirSuperadministrador(this)) return;
            try
            {
                UsuarioRepository.CrearUsuario(
                    txtNombre.Text,
                    txtUsuario.Text,
                    txtContrasena.Text,
                    ddlRol.SelectedValue);
                txtContrasena.Text = String.Empty;
                MostrarMensaje("Cuenta creada correctamente.", false);
                CargarUsuarios();
            }
            catch (ArgumentException ex) { MostrarMensaje(ex.Message, true); }
            catch (Exception) { MostrarMensaje("No se pudo crear la cuenta. Revisa que el usuario no exista y que la base este disponible.", true); }
        }

        private void CargarUsuarios()
        {
            // La consulta devuelve las cuentas para mostrarlas en la grilla.
            try
            {
                gvUsuarios.DataSource = UsuarioRepository.ObtenerUsuarios();
                gvUsuarios.DataBind();
            }
            catch (Exception) { MostrarMensaje("No se pudo cargar la lista de usuarios.", true); }
        }

        private void MostrarMensaje(string texto, bool error)
        {
            lblMensaje.Text = texto;
            lblMensaje.CssClass = error ? "alert alert-danger d-block" : "alert alert-success d-block";
            lblMensaje.Visible = true;
        }
    }
}
