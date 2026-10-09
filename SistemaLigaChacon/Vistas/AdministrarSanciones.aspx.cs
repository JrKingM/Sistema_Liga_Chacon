using System;
using System.Data;
using SistemaLigaChacon.Datos;
using SistemaLigaChacon.Logica;

namespace SistemaLigaChacon.Vistas
{
    public partial class AdministrarSanciones : System.Web.UI.Page
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
                CargarCompetencias();
                CargarPartidos();
                CargarSanciones();
            }
        }

        protected void ddlCompetencia_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            CargarPartidos();
            CargarSanciones();
        }

        protected void ddlPartido_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            CargarEquipos();
        }

        protected void ddlEquipo_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            CargarJugadores();
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            if (!SeguridadWeb.RequerirAdministrador(this)) return;

            int partidoId;
            int equipoId;
            int jugadorId;
            short minuto;
            short? minutoSancion = null;

            if (!Int32.TryParse(ddlPartido.SelectedValue, out partidoId) ||
                !Int32.TryParse(ddlEquipo.SelectedValue, out equipoId) ||
                !Int32.TryParse(ddlJugador.SelectedValue, out jugadorId) ||
                (txtMinuto.Text.Trim().Length > 0 &&
                    (!Int16.TryParse(txtMinuto.Text, out minuto) ||
                     minuto < 0 || minuto > 120)))
            {
                MostrarMensaje(
                    "Selecciona el partido, equipo y jugador. " +
                    "El minuto debe estar entre 0 y 120.",
                    true);
                return;
            }

            if (txtMinuto.Text.Trim().Length > 0)
            {
                Int16.TryParse(txtMinuto.Text, out minuto);
                minutoSancion = minuto;
            }

            try
            {
                SancionesRepository.RegistrarSancion(
                    partidoId,
                    equipoId,
                    jugadorId,
                    ddlTipo.SelectedValue,
                    minutoSancion,
                    txtDescripcion.Text);

                txtMinuto.Text = String.Empty;
                txtDescripcion.Text = String.Empty;
                MostrarMensaje("La sancion se registro correctamente.", false);
                CargarSanciones();
            }
            catch (Exception)
            {
                MostrarMensaje(
                    "No se pudo guardar la sancion. Comprueba que el jugador " +
                    "pertenezca al equipo seleccionado en ese partido.",
                    true);
            }
        }

        private void CargarCompetencias()
        {
            ddlCompetencia.DataSource = SancionesRepository.ObtenerCompetencias();
            ddlCompetencia.DataTextField = "Descripcion";
            ddlCompetencia.DataValueField = "CompetenciaId";
            ddlCompetencia.DataBind();
        }

        private void CargarPartidos()
        {
            int competenciaId;
            if (!Int32.TryParse(ddlCompetencia.SelectedValue, out competenciaId))
            {
                LimpiarLista(ddlPartido);
                LimpiarLista(ddlEquipo);
                LimpiarLista(ddlJugador);
                return;
            }

            ddlPartido.DataSource =
                SancionesRepository.ObtenerPartidos(competenciaId);
            ddlPartido.DataTextField = "Descripcion";
            ddlPartido.DataValueField = "PartidoId";
            ddlPartido.DataBind();
            CargarEquipos();
        }

        private void CargarEquipos()
        {
            int partidoId;
            if (!Int32.TryParse(ddlPartido.SelectedValue, out partidoId))
            {
                LimpiarLista(ddlEquipo);
                LimpiarLista(ddlJugador);
                return;
            }

            ddlEquipo.DataSource =
                SancionesRepository.ObtenerEquiposDelPartido(partidoId);
            ddlEquipo.DataTextField = "Nombre";
            ddlEquipo.DataValueField = "InscripcionEquipoId";
            ddlEquipo.DataBind();
            CargarJugadores();
        }

        private void CargarJugadores()
        {
            int equipoId;
            if (!Int32.TryParse(ddlEquipo.SelectedValue, out equipoId))
            {
                LimpiarLista(ddlJugador);
                pnlSinJugadores.Visible = false;
                btnGuardar.Enabled = false;
                return;
            }

            DataTable jugadores = SancionesRepository.ObtenerJugadores(equipoId);
            ddlJugador.Items.Clear();
            ddlJugador.Items.Add(
                new System.Web.UI.WebControls.ListItem(
                    "Selecciona un jugador",
                    String.Empty));
            ddlJugador.DataSource = jugadores;
            ddlJugador.DataTextField = "Nombre";
            ddlJugador.DataValueField = "InscripcionJugadorId";
            ddlJugador.DataBind();
            pnlSinJugadores.Visible = jugadores.Rows.Count == 0;
            btnGuardar.Enabled = jugadores.Rows.Count > 0;
        }

        private void CargarSanciones()
        {
            int competenciaId;
            if (!Int32.TryParse(ddlCompetencia.SelectedValue, out competenciaId))
            {
                gvSanciones.DataSource = null;
                gvSanciones.DataBind();
                return;
            }

            gvSanciones.DataSource =
                SancionesRepository.ObtenerSanciones(competenciaId);
            gvSanciones.DataBind();
        }

        private static void LimpiarLista(
            System.Web.UI.WebControls.DropDownList lista)
        {
            lista.Items.Clear();
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
