using System;
using System.Data;
using SistemaLigaChacon.Datos;

namespace SistemaLigaChacon.Vistas
{
    public partial class Posiciones : System.Web.UI.Page
    {
        // Lee la categoria elegida de la URL y usa Mayor como opcion inicial.
        private string CategoriaSeleccionada
        {
            get
            {
                string categoria = (Request.QueryString["categoria"] ?? "Mayor").Trim();
                return String.Equals(categoria, "Intermedia", StringComparison.OrdinalIgnoreCase)
                    ? "Intermedia"
                    : "Mayor";
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Prepara el selector para que el usuario pueda cambiar categoria.
            litCategoria.Text = CategoriaSeleccionada;
            lnkMayor.NavigateUrl = ResolveUrl("~/Vistas/Posiciones.aspx?categoria=Mayor");
            lnkIntermedia.NavigateUrl = ResolveUrl("~/Vistas/Posiciones.aspx?categoria=Intermedia");
            lnkMayor.CssClass = CategoriaSeleccionada == "Mayor"
                ? "btn btn-success active"
                : "btn btn-outline-success";
            lnkIntermedia.CssClass = CategoriaSeleccionada == "Intermedia"
                ? "btn btn-success active"
                : "btn btn-outline-success";

            try
            {
                // La tabla se calcula en SQL desde los partidos finalizados.
                DataTable posiciones = LigaRepository.ObtenerTablaPosiciones(CategoriaSeleccionada);
                rptPosiciones.DataSource = posiciones;
                rptPosiciones.DataBind();
                pnlVacio.Visible = posiciones.Rows.Count == 0;
            }
            catch (Exception)
            {
                lblError.Text =
                    "No se pudo cargar la tabla. Verifica la conexion y que exista " +
                    "la vista vw_TablaPosiciones en la base de datos.";
                lblError.Visible = true;
                pnlVacio.Visible = false;
            }
        }
    }
}
