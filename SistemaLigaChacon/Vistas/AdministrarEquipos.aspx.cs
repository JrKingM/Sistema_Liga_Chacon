using System;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using SistemaLigaChacon.Datos;
using SistemaLigaChacon.Logica;

namespace SistemaLigaChacon.Vistas
{
    public partial class AdministrarEquipos : System.Web.UI.Page
    {
        protected void Page_Init(object sender, EventArgs e)
        {
            ViewStateUserKey = Session.SessionID;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!SeguridadWeb.RequerirAdministrador(this)) return;

            if (!IsPostBack)
            {
                CargarCategorias();
                CargarEquipos();
                CargarEquiposParaPlantilla();
            }
        }

        protected void ddlCategoria_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            CargarEquipos();
            CargarEquiposParaPlantilla();
        }

        protected void ddlEquipoPlantilla_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            CargarPlantilla();
        }

        protected void btnCrear_Click(object sender, EventArgs e)
        {
            if (!SeguridadWeb.RequerirAdministrador(this)) return;

            int categoriaId;
            if (!Int32.TryParse(ddlCategoria.SelectedValue, out categoriaId))
            {
                MostrarMensaje(
                    "No hay una competencia activa para seleccionar.",
                    true);
                return;
            }

            string imagenGuardada = null;
            bool equipoCreado = false;
            try
            {
                string logoUrl = GuardarOValidarEscudo(out imagenGuardada);
                EquiposAdministracionRepository.CrearEquipo(
                    txtCodigo.Text,
                    txtNombre.Text,
                    txtRepresentante.Text,
                    txtTelefono.Text,
                    logoUrl,
                    categoriaId);
                equipoCreado = true;

                txtCodigo.Text = String.Empty;
                txtNombre.Text = String.Empty;
                txtRepresentante.Text = String.Empty;
                txtTelefono.Text = String.Empty;

                MostrarMensaje("El equipo se agrego a la competencia.", false);
                CargarEquipos();
                CargarEquiposParaPlantilla();
            }
            catch (ArgumentException ex)
            {
                BorrarImagenSiFallo(imagenGuardada, equipoCreado);
                MostrarMensaje(ex.Message, true);
            }
            catch (Exception)
            {
                BorrarImagenSiFallo(imagenGuardada, equipoCreado);
                MostrarMensaje(
                    "No se pudo agregar el equipo. Revisa si el codigo o el " +
                    "nombre ya existen, o si la competencia ya tiene 12 equipos.",
                    true);
            }
        }

        // Guarda el escudo seleccionado desde el dispositivo despues de validar el archivo.
        private string GuardarOValidarEscudo(out string rutaGuardada)
        {
            rutaGuardada = null;
            if (!fuLogo.HasFile)
                return null;

            const int tamanoMaximo = 3 * 1024 * 1024;
            if (fuLogo.PostedFile.ContentLength <= 0 ||
                fuLogo.PostedFile.ContentLength > tamanoMaximo)
            {
                throw new ArgumentException(
                    "La imagen debe pesar mas de 0 y como maximo 3 MB.");
            }

            byte[] contenido;
            using (var memoria = new MemoryStream())
            {
                fuLogo.PostedFile.InputStream.CopyTo(memoria);
                contenido = memoria.ToArray();
            }

            string extension;
            using (var memoria = new MemoryStream(contenido))
            using (var imagen = Image.FromStream(memoria, true, true))
            {
                if (imagen.Width > 4096 || imagen.Height > 4096 ||
                    (long)imagen.Width * imagen.Height > 16000000)
                {
                    throw new ArgumentException(
                        "La resolucion maxima del escudo es 4096 por 4096 pixeles.");
                }

                if (imagen.RawFormat.Guid == ImageFormat.Jpeg.Guid)
                    extension = ".jpg";
                else if (imagen.RawFormat.Guid == ImageFormat.Png.Guid)
                    extension = ".png";
                else if (imagen.RawFormat.Guid == ImageFormat.Gif.Guid)
                    extension = ".gif";
                else
                    throw new ArgumentException(
                        "El formato debe ser JPG, PNG o GIF.");
            }

            string carpeta = Server.MapPath("~/Images/Equipos");
            Directory.CreateDirectory(carpeta);

            string nombreArchivo = Guid.NewGuid().ToString("N") + extension;
            string rutaFisica = Path.Combine(carpeta, nombreArchivo);
            File.WriteAllBytes(rutaFisica, contenido);

            rutaGuardada = "~/Images/Equipos/" + nombreArchivo;
            return rutaGuardada;
        }

        private void BorrarImagenSiFallo(string rutaRelativa, bool equipoCreado)
        {
            if (equipoCreado || String.IsNullOrWhiteSpace(rutaRelativa)) return;

            string rutaFisica = Server.MapPath(rutaRelativa);
            if (File.Exists(rutaFisica))
                File.Delete(rutaFisica);
        }

        protected void btnAgregarJugador_Click(object sender, EventArgs e)
        {
            if (!SeguridadWeb.RequerirAdministrador(this)) return;

            int inscripcionEquipoId;
            short dorsal;
            short? dorsalOpcional = null;
            if (!Int32.TryParse(
                    ddlEquipoPlantilla.SelectedValue,
                    out inscripcionEquipoId) ||
                (txtDorsal.Text.Trim().Length > 0 &&
                    (!Int16.TryParse(txtDorsal.Text, out dorsal) ||
                     dorsal < 1 || dorsal > 99)))
            {
                MostrarMensaje(
                    "Selecciona un equipo. El dorsal debe ser de 1 a 99.",
                    true);
                return;
            }

            if (txtDorsal.Text.Trim().Length > 0)
            {
                Int16.TryParse(txtDorsal.Text, out dorsal);
                dorsalOpcional = dorsal;
            }

            try
            {
                EquiposAdministracionRepository.AgregarJugador(
                    txtNombresJugador.Text,
                    txtApellidosJugador.Text,
                    dorsalOpcional,
                    inscripcionEquipoId);

                txtNombresJugador.Text = String.Empty;
                txtApellidosJugador.Text = String.Empty;
                txtDorsal.Text = String.Empty;
                MostrarMensaje("El jugador se agrego a la plantilla.", false);
                CargarPlantilla();
            }
            catch (ArgumentException ex)
            {
                MostrarMensaje(ex.Message, true);
            }
            catch (Exception)
            {
                MostrarMensaje(
                    "No se pudo agregar el jugador a la plantilla.",
                    true);
            }
        }

        private void CargarCategorias()
        {
            DataTable categorias =
                EquiposAdministracionRepository.ObtenerCategoriasActivas();
            ddlCategoria.DataSource = categorias;
            ddlCategoria.DataTextField = "Descripcion";
            ddlCategoria.DataValueField = "CategoriaId";
            ddlCategoria.DataBind();
            btnCrear.Enabled = categorias.Rows.Count > 0;
        }

        private void CargarEquipos()
        {
            int categoriaId;
            if (!Int32.TryParse(ddlCategoria.SelectedValue, out categoriaId))
            {
                gvEquipos.DataSource = null;
                gvEquipos.DataBind();
                return;
            }

            gvEquipos.DataSource =
                EquiposAdministracionRepository.ObtenerEquipos(categoriaId);
            gvEquipos.DataBind();
        }

        private void CargarEquiposParaPlantilla()
        {
            int categoriaId;
            if (!Int32.TryParse(ddlCategoria.SelectedValue, out categoriaId))
            {
                ddlEquipoPlantilla.Items.Clear();
                gvPlantilla.DataSource = null;
                gvPlantilla.DataBind();
                return;
            }

            ddlEquipoPlantilla.DataSource =
                EquiposAdministracionRepository
                    .ObtenerInscripcionesEquipo(categoriaId);
            ddlEquipoPlantilla.DataTextField = "Nombre";
            ddlEquipoPlantilla.DataValueField = "InscripcionEquipoId";
            ddlEquipoPlantilla.DataBind();
            CargarPlantilla();
        }

        private void CargarPlantilla()
        {
            int inscripcionEquipoId;
            if (!Int32.TryParse(
                ddlEquipoPlantilla.SelectedValue,
                out inscripcionEquipoId))
            {
                gvPlantilla.DataSource = null;
                gvPlantilla.DataBind();
                return;
            }

            gvPlantilla.DataSource =
                EquiposAdministracionRepository
                    .ObtenerPlantilla(inscripcionEquipoId);
            gvPlantilla.DataBind();
        }

        private void MostrarMensaje(string texto, bool error)
        {
            lblMensaje.Text = texto;
            lblMensaje.CssClass = error
                ? "alert alert-danger d-block"
                : "alert alert-success d-block";
            lblMensaje.Visible = true;
        }
    }
}
