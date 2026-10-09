using System;
using SistemaLigaChacon.Datos;
using SistemaLigaChacon.Logica;

namespace SistemaLigaChacon.Vistas
{
    public partial class Campeones : System.Web.UI.Page
    {
        private bool EsAdministrador
        {
            get
            {
                return SeguridadWeb.EsAdministrador(Session);
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // El formulario de archivo solo se muestra a administradores.
            pnlAdmin.Visible = EsAdministrador;
            pnlNoAdmin.Visible = !EsAdministrador;
            CargarCampeones();
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            // No basta con ocultar el formulario: el servidor valida el rol.
            if (!EsAdministrador)
            {
                Response.StatusCode = 403;
                return;
            }

            var temporada = (txtTemporada.Text ?? String.Empty).Trim();
            var equipo = (txtEquipo.Text ?? String.Empty).Trim();
            var portero = (txtPortero.Text ?? String.Empty).Trim();
            var goleador = (txtGoleador.Text ?? String.Empty).Trim();
            var categoria = ddlCategoria.SelectedValue;

            if (temporada.Length == 0 || equipo.Length == 0 || (categoria != "Mayor" && categoria != "Intermedia"))
            {
                MostrarMensaje("Completa la temporada, categoria y equipo campeon.", true);
                return;
            }

            // El repositorio verifica duplicados y guarda el registro historico.
            var agregado = CampeonRepository.Agregar(new CampeonTemporada
            {
                Temporada = temporada,
                Categoria = categoria,
                Equipo = equipo,
                Portero = portero,
                Goleador = goleador,
                ArchivadoUtc = DateTime.UtcNow
            });

            if (!agregado)
            {
                MostrarMensaje("Ya existe un campeon archivado para esa temporada y categoria.", true);
                return;
            }

            txtEquipo.Text = String.Empty;
            txtPortero.Text = String.Empty;
            txtGoleador.Text = String.Empty;
            MostrarMensaje("Campeon archivado correctamente.", false);
            CargarCampeones();
        }

        private void CargarCampeones()
        {
            // Muestra los campeones archivados aunque ya termino la temporada.
            var campeones = CampeonRepository.ObtenerTodos();
            rptCampeones.DataSource = campeones;
            rptCampeones.DataBind();
            pnlVacio.Visible = campeones.Count == 0;
        }

        private void MostrarMensaje(string mensaje, bool esError)
        {
            lblResultado.Text = mensaje;
            lblResultado.CssClass = esError ? "message error" : "message";
        }
    }
}
