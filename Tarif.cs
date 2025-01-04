using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YazLab1
{
    public class Tarif
    {
        public int ID { get; set; }
        public string ad { get; set; }
        public string kategori { get; set; }
        public string hazirlamaTalimatlari { get; set; }
        public int hazirlanisSuresi { get; set; }
        public Dictionary<Malzeme, int> malzemeler { get; set; }

        public Tarif()
        {
            malzemeler = new Dictionary<Malzeme, int>();
        }

        public decimal Maliyet
        {
            get
            {
                decimal totalCost = 0;

                foreach (var malzeme in malzemeler)
                {
                    totalCost += malzeme.Key.birimFiyat * malzeme.Value;
                }

                return totalCost;
            }
        }

        public int ToplamMalzemeSayisi
        {
            get
            {
                int toplamMalzemeSayisi = 0;   
                
                foreach(var malzeme in malzemeler)
                {
                    toplamMalzemeSayisi += malzeme.Value;
                }

                return toplamMalzemeSayisi;
            }
        }
    }
}
