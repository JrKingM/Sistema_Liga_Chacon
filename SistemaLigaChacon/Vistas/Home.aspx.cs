using System;
using System.Data;
using SistemaLigaChacon.Datos;

namespace SistemaLigaChacon.Vistas
{
    public partial class Home : System.Web.UI.Page
    {
        // La primera carga consulta los datos. En postbacks se conservan los
        // controles para no repetir consultas al servidor sin necesidad.
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarDatosPublicos();
            }
        }

        // La categoria llega en la URL. Si no es Intermedia, se usa Mayor.
        private string CategoriaSeleccionada
        {
            get
            {
                return String.Equals(Request.QueryString["categoria"], "Intermedia", StringComparison.OrdinalIgnoreCase)
                    ? "Intermedia"
                    : "Mayor";
            }
        }

        private void CargarDatosPublicos()
        {
            try
            {
                // El repositorio consulta SQL y devuelve tablas que sirven
                // como origen de datos para los controles de esta pagina.
                DataTable equiposMayor = LigaRepository.ObtenerEquipos("Mayor");
                DataTable equiposIntermedia = LigaRepository.ObtenerEquipos("Intermedia");
                DataTable posiciones = LigaRepository
                    .ObtenerTablaPosiciones(CategoriaSeleccionada);
                DataTable proximos = LigaRepository
                    .ObtenerPartidosProximos(5, CategoriaSeleccionada);
                DataTable resultados = LigaRepository
                    .ObtenerUltimosResultados(5, CategoriaSeleccionada);

                // Actualiza los textos y enlaces del selector de categoria.
                litClubCount.Text = (equiposMayor.Rows.Count + equiposIntermedia.Rows.Count).ToString();
                litCategoria.Text = CategoriaSeleccionada;
                lnkMayor.NavigateUrl = ResolveUrl(
                    "~/Vistas/Home.aspx?categoria=Mayor");
                lnkIntermedia.NavigateUrl = ResolveUrl(
                    "~/Vistas/Home.aspx?categoria=Intermedia");
                lnkMayor.CssClass = CategoriaSeleccionada == "Mayor"
                    ? "btn btn-success active"
                    : "btn btn-outline-success";
                lnkIntermedia.CssClass = CategoriaSeleccionada == "Intermedia"
                    ? "btn btn-success active"
                    : "btn btn-outline-success";

                // Enlaza cada tabla al repetidor que la dibuja en el .aspx.
                rptEquiposMayor.DataSource = equiposMayor;
                rptEquiposMayor.DataBind();
                rptEquiposIntermedia.DataSource = equiposIntermedia;
                rptEquiposIntermedia.DataBind();
                rptPosiciones.DataSource = posiciones;
                rptPosiciones.DataBind();
                rptProximos.DataSource = proximos;
                rptProximos.DataBind();
                rptResultados.DataSource = resultados;
                rptResultados.DataBind();

                // Si una consulta no trae filas, el .aspx muestra su aviso vacio.
                pnlPosicionesVacias.Visible = posiciones.Rows.Count == 0;
                pnlProximosVacios.Visible = proximos.Rows.Count == 0;
                pnlResultadosVacios.Visible = resultados.Rows.Count == 0;
            }
            catch (Exception)
            {
                // Evita mostrar detalles internos de SQL al visitante.
                lblError.Text =
                    "No se pudo cargar la informacion de la liga. " +
                    "Verifica la conexion y los permisos de la base de datos.";
                lblError.Visible = true;
            }
        }
    }
}
