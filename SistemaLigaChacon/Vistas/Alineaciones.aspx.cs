using System;
using System.Data;
using SistemaLigaChacon.Datos;
using SistemaLigaChacon.Logica;

namespace SistemaLigaChacon.Vistas
{
    public partial class Alineaciones : System.Web.UI.Page
    {
        protected void Page_Init(object sender, EventArgs e)
        {
            ViewStateUserKey = Session.SessionID;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            pnlAdministracion.Visible = SeguridadWeb.EsAdministrador(Session);

            if (!IsPostBack)
            {
                CargarCompetencias();
                CargarPartidos();
                CargarEquipos();
            }
        }

        protected void ddlCompetencia_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            CargarPartidos();
            CargarEquipos();
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
            CargarAlineacion();
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            if (!SeguridadWeb.RequerirAdministrador(this)) return;

            int partidoId;
            int equipoId;
            int jugadorId;
            if (!Int32.TryParse(ddlPartido.SelectedValue, out partidoId) ||
                !Int32.TryParse(ddlEquipo.SelectedValue, out equipoId) ||
                !Int32.TryParse(ddlJugador.SelectedValue, out jugadorId))
            {
                MostrarMensaje(
                    "Selecciona un partido, equipo y jugador de la plantilla.",
                    true);
                return;
            }

            try
            {
                AlineacionesRepository.GuardarJugadorEnPartido(
                    partidoId,
                    equipoId,
                    jugadorId,
                    ddlTipo.SelectedValue,
                    txtPosicion.Text,
                    chkEsPortero.Checked);

                MostrarMensaje("La alineacion se guardo correctamente.", false);
                CargarAlineacion();
            }
            catch (Exception)
            {
                MostrarMensaje(
                    "No se pudo guardar. Comprueba que el jugador pertenezca " +
                    "al equipo seleccionado y que no haya mas de 11 titulares.",
                    true);
            }
        }

        private void CargarCompetencias()
        {
            ddlCompetencia.DataSource = AlineacionesRepository.ObtenerCompetencias();
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
                CargarAlineacion();
                return;
            }

            ddlPartido.DataSource =
                AlineacionesRepository.ObtenerPartidos(competenciaId);
            ddlPartido.DataTextField = "Descripcion";
            ddlPartido.DataValueField = "PartidoId";
            ddlPartido.DataBind();
        }

        private void CargarEquipos()
        {
            int partidoId;
            if (!Int32.TryParse(ddlPartido.SelectedValue, out partidoId))
            {
                LimpiarLista(ddlEquipo);
                LimpiarLista(ddlJugador);
                CargarAlineacion();
                return;
            }

            ddlEquipo.DataSource = AlineacionesRepository.ObtenerEquipos(partidoId);
            ddlEquipo.DataTextField = "Nombre";
            ddlEquipo.DataValueField = "InscripcionEquipoId";
            ddlEquipo.DataBind();
            CargarJugadores();
            CargarAlineacion();
        }

        private void CargarJugadores()
        {
            int equipoId;
            if (!Int32.TryParse(ddlEquipo.SelectedValue, out equipoId))
            {
                LimpiarLista(ddlJugador);
                return;
            }

            ddlJugador.DataSource = AlineacionesRepository.ObtenerJugadores(equipoId);
            ddlJugador.DataTextField = "Nombre";
            ddlJugador.DataValueField = "InscripcionJugadorId";
            ddlJugador.DataBind();
        }

        private void CargarAlineacion()
        {
            int partidoId;
            int equipoId;
            if (!Int32.TryParse(ddlPartido.SelectedValue, out partidoId) ||
                !Int32.TryParse(ddlEquipo.SelectedValue, out equipoId))
            {
                gvAlineacion.DataSource = null;
                gvAlineacion.DataBind();
                return;
            }

            gvAlineacion.DataSource =
                AlineacionesRepository.ObtenerAlineacion(partidoId, equipoId);
            gvAlineacion.DataBind();
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
