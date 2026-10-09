using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using SistemaLigaChacon.Logica;

namespace SistemaLigaChacon.Datos
{
    public static class CampeonRepository
    {
        private static readonly object Sync = new object();

        private static string Archivo
        {
            get
            {
                var carpeta = HttpContext.Current.Server.MapPath("~/App_Data");
                Directory.CreateDirectory(carpeta);
                return Path.Combine(carpeta, "Campeones.json");
            }
        }

        public static IList<CampeonTemporada> ObtenerTodos()
        {
            lock (Sync)
            {
                var archivo = Archivo;
                if (!File.Exists(archivo)) return new List<CampeonTemporada>();

                var json = File.ReadAllText(archivo);
                var registros = new JavaScriptSerializer().Deserialize<List<CampeonTemporada>>(json);
                return (registros ?? new List<CampeonTemporada>())
                    .OrderByDescending(x => x.ArchivadoUtc)
                    .ToList();
            }
        }

        public static bool Agregar(CampeonTemporada campeon)
        {
            lock (Sync)
            {
                var archivo = Archivo;
                var registros = new List<CampeonTemporada>();
                if (File.Exists(archivo))
                {
                    var guardados = new JavaScriptSerializer().Deserialize<List<CampeonTemporada>>(File.ReadAllText(archivo));
                    if (guardados != null) registros.AddRange(guardados);
                }

                if (registros.Any(x => x.Temporada == campeon.Temporada && x.Categoria == campeon.Categoria))
                    return false;

                registros.Add(campeon);
                var serializador = new JavaScriptSerializer();
                var temporal = archivo + ".tmp";
                File.WriteAllText(temporal, serializador.Serialize(registros));
                if (File.Exists(archivo)) File.Replace(temporal, archivo, null);
                else File.Move(temporal, archivo);
                return true;
            }
        }
    }
}
