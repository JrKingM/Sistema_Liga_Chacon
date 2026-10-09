using System;

namespace SistemaLigaChacon.Logica
{
    [Serializable]
    public class CampeonTemporada
    {
        public string Temporada { get; set; }
        public string Categoria { get; set; }
        public string Equipo { get; set; }
        public string Portero { get; set; }
        public string Goleador { get; set; }
        public DateTime ArchivadoUtc { get; set; }
    }
}
