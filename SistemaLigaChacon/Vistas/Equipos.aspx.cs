using System;
using System.Data;
using SistemaLigaChacon.Datos;

namespace SistemaLigaChacon.Vistas
{
    public partial class Equipos : System.Web.UI.Page
    {
        // Consulta los equipos una vez al abrir la pagina.
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) CargarEquipos();
        }

        private void CargarEquipos()
        {
            try
            {
                // Cada categoria se consulta por separado para evitar mezclar clubes.
                DataTable mayor = LigaRepository.ObtenerEquipos("Mayor");
                DataTable intermedia = LigaRepository.ObtenerEquipos("Intermedia");
                rptEquiposMayor.DataSource = mayor;
                rptEquiposMayor.DataBind();
                rptEquiposIntermedia.DataSource = intermedia;
                rptEquiposIntermedia.DataBind();
                pnlSinEquiposMayor.Visible = mayor.Rows.Count == 0;
                pnlSinEquiposIntermedia.Visible = intermedia.Rows.Count == 0;
            }
            catch (Exception)
            {
                lblError.Text =
                    "No se pudieron cargar los equipos. " +
                    "Verifica la conexion y los permisos de la base de datos.";
                lblError.Visible = true;
            }
        }
    }
}
