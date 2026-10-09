using System;
using System.Data;
using System.Globalization;
using System.Web.UI.WebControls;
using SistemaLigaChacon.Datos;
using SistemaLigaChacon.Logica;

namespace SistemaLigaChacon.Vistas
{
    public partial class AdministrarPartidos : System.Web.UI.Page
    {
        // Vincula el estado de la pagina a la sesion del administrador.
        protected void Page_Init(object sender, EventArgs e)
        {
            ViewStateUserKey = Session.SessionID;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Protege tambien los postbacks y marca la cancha como obligatoria.
            if (!SeguridadWeb.RequerirAdministrador(this)) return;
            ddlCancha.Attributes["required"] = "required";

            // Los datos iniciales se cargan una vez; los postbacks conservan
            // los valores seleccionados por el administrador.
            if (!IsPostBack)
            {
                txtFecha.Text = DateTime.Now.AddDays(7).ToString("yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture);
                CargarCompetencias();
            }
        }

        protected void ddlCompetencia_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!SeguridadWeb.RequerirAdministrador(this)) return;

            // Cambiar competencia cierra cualquier edicion anterior y recarga
            // jornadas, equipos inscritos y partidos de la nueva competencia.
            SalirEdicion();
            CargarDetalles();
        }

        protected void btnCrearPartido_Click(object sender, EventArgs e)
        {
            if (!SeguridadWeb.RequerirAdministrador(this)) return;

            // Convierte los valores del formulario y rechaza entradas invalidas
            // antes de intentar escribir en SQL Server.
            int competenciaId, jornada, local, visita;
            DateTime fecha;
            if (!Int32.TryParse(ddlCompetencia.SelectedValue, out competenciaId) ||
                !Int32.TryParse(ddlJornada.SelectedValue, out jornada) ||
                !Int32.TryParse(ddlLocal.SelectedValue, out local) ||
                !Int32.TryParse(ddlVisita.SelectedValue, out visita) || local == visita ||
                String.IsNullOrWhiteSpace(ddlCancha.SelectedValue) ||
                !DateTime.TryParseExact(
                    txtFecha.Text,
                    "yyyy-MM-ddTHH:mm",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out fecha))
            {
                MostrarMensaje(
                    "Selecciona competencia, jornada, cancha, dos equipos distintos y una fecha valida.",
                    true);
                return;
            }

            // El repositorio hace una segunda validacion y guarda el encuentro
            // como PENDIENTE. Despues se actualiza la lista visible.
            try
            {
                AdministracionRepository.CrearPartido(
                    competenciaId,
                    jornada,
                    local,
                    visita,
                    fecha,
                    ddlCancha.SelectedValue);
                MostrarMensaje(
                    "Partido programado. Al guardar el marcador final " +
                    "se actualizara la tabla de posiciones.",
                    false);
                CargarDetalles();
            }
            catch (Exception)
            {
                MostrarMensaje(
                    "No se pudo guardar el partido. Verifica que la jornada y los dos equipos pertenezcan a la competencia.",
                    true);
            }
        }

        // Un solo evento de grilla atiende editar programacion, borrar partido
        // pendiente y guardar marcador; CommandName identifica la accion.
        protected void gvPartidos_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (!SeguridadWeb.RequerirAdministrador(this)) return;
            var boton = e.CommandSource as Button;
            var fila = boton == null ? null : boton.NamingContainer as GridViewRow;
            int partidoId;
            if (fila == null || !Int32.TryParse(Convert.ToString(e.CommandArgument), out partidoId)) return;

            if (e.CommandName == "EditarProgramacion")
            {
                CargarEdicion(partidoId);
                return;
            }

            if (e.CommandName == "EliminarPartido")
            {
                // Se usa la version cargada en la grilla para evitar borrar una
                // fila que otro administrador haya actualizado entretanto.
                try
                {
                    var campoVersion = (HiddenField)fila.FindControl("hfVersionFila");
                    byte[] versionBorrar = Convert.FromBase64String(campoVersion.Value);
                    if (AdministracionRepository.EliminarPartidoPendiente(partidoId, versionBorrar))
                    {
                        SalirEdicion();
                        MostrarMensaje("Partido pendiente eliminado.", false);
                    }
                    else
                    {
                        MostrarMensaje(
                            "No se elimino: el partido ya cambio o ya tiene resultado. Los partidos finalizados se conservan.",
                            true);
                    }
                    CargarPartidos();
                }
                catch (Exception)
                {
                    MostrarMensaje(
                        "No se pudo eliminar el partido. Si tiene eventos registrados, el sistema conserva el registro.",
                        true);
                }
                return;
            }

            if (e.CommandName != "GuardarResultado") return;

            // El marcador acepta solo goles enteros dentro del rango permitido.
            int golesLocal, golesVisita;
            if (!Int32.TryParse(((TextBox)fila.FindControl("txtGolesLocal")).Text, out golesLocal) ||
                !Int32.TryParse(((TextBox)fila.FindControl("txtGolesVisita")).Text, out golesVisita) ||
                golesLocal < 0 || golesLocal > 99 || golesVisita < 0 || golesVisita > 99)
            {
                MostrarMensaje("Escribe ambos goles como numeros entre 0 y 99.", true);
                return;
            }

            try
            {
                // Si la version ya cambio, se avisa para que se revise el dato
                // actual antes de volver a guardar.
                var campoVersion = (HiddenField)fila.FindControl("hfVersionFila");
                byte[] version = Convert.FromBase64String(campoVersion.Value);
                if (!AdministracionRepository.GuardarResultado(partidoId, golesLocal, golesVisita, version))
                    MostrarMensaje(
                        "Otro usuario modifico este partido. Se actualizaron los datos; revisa el marcador y vuelve a guardar.",
                        true);
                else
                    MostrarMensaje("Resultado guardado. La tabla de posiciones se recalcula automaticamente.", false);
                SalirEdicion();
                CargarPartidos();
            }
            catch (Exception)
            {
                MostrarMensaje(
                    "No se pudo guardar el resultado. Recarga la pagina y vuelve a intentarlo.",
                    true);
            }
        }

        protected void btnActualizarPartido_Click(object sender, EventArgs e)
        {
            if (!SeguridadWeb.RequerirAdministrador(this)) return;

            // Comprueba que los valores del formulario de edicion sean validos.
            int partidoId, competenciaId, jornada, local, visita;
            DateTime fecha;
            if (!Int32.TryParse(hfPartidoEditarId.Value, out partidoId) ||
                !Int32.TryParse(ddlCompetencia.SelectedValue, out competenciaId) ||
                !Int32.TryParse(ddlJornada.SelectedValue, out jornada) ||
                !Int32.TryParse(ddlLocal.SelectedValue, out local) ||
                !Int32.TryParse(ddlVisita.SelectedValue, out visita) || local == visita ||
                String.IsNullOrWhiteSpace(ddlCancha.SelectedValue) ||
                !DateTime.TryParseExact(
                    txtFecha.Text,
                    "yyyy-MM-ddTHH:mm",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out fecha))
            {
                MostrarMensaje("Revisa jornada, cancha, fecha y equipos distintos.", true);
                return;
            }

            try
            {
                // El repositorio solo actualiza si sigue pendiente y conserva
                // la version que se cargo al iniciar la edicion.
                byte[] version = Convert.FromBase64String(hfVersionEdicion.Value);
                bool actualizado = AdministracionRepository.ActualizarProgramacion(
                    partidoId,
                    competenciaId,
                    jornada,
                    local,
                    visita,
                    fecha,
                    ddlCancha.SelectedValue,
                    version);

                if (!actualizado)
                {
                    MostrarMensaje(
                        "No se actualizaron los datos. El partido ya cambio o ya no esta pendiente; recarga y revisa.",
                        true);
                }
                else
                {
                    MostrarMensaje("Programacion del partido actualizada.", false);
                }
                SalirEdicion();
                CargarDetalles();
            }
            catch (Exception)
            {
                MostrarMensaje(
                    "No se pudo actualizar el partido. Verifica sus datos " +
                    "y vuelve a cargar la pagina.",
                    true);
            }
        }

        protected void btnCancelarEdicion_Click(object sender, EventArgs e)
        {
            if (!SeguridadWeb.RequerirAdministrador(this)) return;
            SalirEdicion();
            CargarDetalles();
        }

        private void CargarEdicion(int partidoId)
        {
            // Recupera el registro completo para llenar el formulario de edicion.
            int competenciaId;
            if (!Int32.TryParse(ddlCompetencia.SelectedValue, out competenciaId)) return;
            try
            {
                DataTable datos = AdministracionRepository.ObtenerPartidoParaEdicion(partidoId, competenciaId);
                if (datos.Rows.Count == 0)
                {
                    MostrarMensaje("No se encontro el partido en esta competencia.", true);
                    return;
                }
                DataRow partido = datos.Rows[0];
                if (!String.Equals(
                    Convert.ToString(partido["Estado"]),
                    "PENDIENTE",
                    StringComparison.OrdinalIgnoreCase))
                {
                    MostrarMensaje(
                        "Solo puedes editar la programacion antes del resultado. " +
                        "Para corregir el marcador, usa Guardar resultado.",
                        true);
                    return;
                }
                ddlJornada.SelectedValue = Convert.ToString(partido["Jornada"]);
                ddlLocal.SelectedValue = Convert.ToString(partido["EquipoLocalInscripcionId"]);
                ddlVisita.SelectedValue = Convert.ToString(partido["EquipoVisitaInscripcionId"]);
                txtFecha.Text = Convert.ToDateTime(partido["FechaHora"])
                    .ToString("yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture);
                string cancha = Convert.ToString(partido["Cancha"]);
                ddlCancha.SelectedValue = ddlCancha.Items.FindByValue(cancha) == null ? String.Empty : cancha;
                hfPartidoEditarId.Value = Convert.ToString(partidoId);
                hfVersionEdicion.Value = Convert.ToBase64String((byte[])partido["VersionFila"]);
                btnCrearPartido.Visible = false;
                btnActualizarPartido.Visible = true;
                btnCancelarEdicion.Visible = true;
                litModoFormulario.Text = "Editar partido pendiente #" + partidoId;
                MostrarMensaje("Edita la programacion y guarda los cambios.", false);
            }
            catch (Exception) { MostrarMensaje("No se pudo cargar el partido para editar.", true); }
        }

        private void SalirEdicion()
        {
            // Limpia identificadores y vuelve el formulario al modo de alta.
            hfPartidoEditarId.Value = String.Empty;
            hfVersionEdicion.Value = String.Empty;
            btnCrearPartido.Visible = true;
            btnActualizarPartido.Visible = false;
            btnCancelarEdicion.Visible = false;
            litModoFormulario.Text = "Programar partido";
        }

        private void CargarCompetencias()
        {
            try
            {
                // Llena el selector principal. Sin competencias no se pueden
                // seleccionar jornadas ni equipos.
                ddlCompetencia.DataSource = AdministracionRepository.ObtenerCompetencias();
                ddlCompetencia.DataTextField = "Descripcion";
                ddlCompetencia.DataValueField = "CompetenciaId";
                ddlCompetencia.DataBind();
                if (ddlCompetencia.Items.Count == 0)
                {
                    MostrarMensaje(
                        "No existen competencias en la base. Crea primero una temporada, " +
                        "categoria e inscripciones de equipos.",
                        true);
                    btnCrearPartido.Enabled = false;
                    return;
                }
                CargarDetalles();
            }
            catch (Exception) { MostrarMensaje("No se pudieron cargar las competencias desde la base de datos.", true); }
        }

        private void CargarDetalles()
        {
            // Para la competencia elegida carga jornadas, participantes y
            // despues la lista de partidos.
            int competenciaId;
            if (!Int32.TryParse(ddlCompetencia.SelectedValue, out competenciaId)) return;
            try
            {
                AdministracionRepository.AsegurarJornadas(competenciaId);
                ddlJornada.DataSource = AdministracionRepository.ObtenerJornadas(competenciaId);
                ddlJornada.DataTextField = "Numero";
                ddlJornada.DataValueField = "Numero";
                ddlJornada.DataBind();
                var equipos = AdministracionRepository.ObtenerEquiposCompetencia(competenciaId);
                ddlLocal.DataSource = equipos;
                ddlLocal.DataTextField = "Nombre";
                ddlLocal.DataValueField = "InscripcionEquipoId";
                ddlLocal.DataBind();
                ddlVisita.DataSource = equipos.Copy();
                ddlVisita.DataTextField = "Nombre";
                ddlVisita.DataValueField = "InscripcionEquipoId";
                ddlVisita.DataBind();
                btnCrearPartido.Enabled = equipos.Rows.Count >= 2 && ddlJornada.Items.Count > 0;
                CargarPartidos();
            }
            catch (Exception) { MostrarMensaje("No se pudieron cargar las jornadas o equipos de la competencia.", true); }
        }

        private void CargarPartidos()
        {
            // Vuelve a consultar la grilla despues de crear, editar o borrar.
            int competenciaId;
            if (!Int32.TryParse(ddlCompetencia.SelectedValue, out competenciaId)) return;
            gvPartidos.DataSource = AdministracionRepository.ObtenerPartidosCompetencia(competenciaId);
            gvPartidos.DataBind();
        }

        private void MostrarMensaje(string texto, bool error)
        {
            lblMensaje.Text = texto;
            lblMensaje.CssClass = error ? "alert alert-danger d-block" : "alert alert-success d-block";
            lblMensaje.Visible = true;
        }
    }
}
