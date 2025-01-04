using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YazLab1
{
    public class Malzeme
    {
        public int ID { get; set; }
        public string ad { get; set; }
        public string toplamMiktar { get; set; }
        public string birim { get; set; }
        public decimal birimFiyat { get; set; }
    }
}
