using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;

namespace YazLab1
{
    public class FiltrelemeIslemleri
    {
        DBMethods dBMethods = null;

        public FiltrelemeIslemleri()
        {
            dBMethods = new DBMethods();
        }

        public DataTable MalzemeSayisinaGoreSirala(DataTable siralanacakTarifler)
        {
            List<Tarif> tarifListesi = new List<Tarif>();

            foreach (DataRow row in siralanacakTarifler.Rows)
            {
                Tarif tarif = new Tarif
                {
                    ID = Convert.ToInt32(row["TarifID"]),
                    ad = row["TarifAdi"].ToString(),
                    kategori = row["Kategori"].ToString(),
                    hazirlanisSuresi = Convert.ToInt32(row["HazirlamaSuresi"]),
                    hazirlamaTalimatlari = row["Talimatlar"].ToString(),
                    malzemeler = dBMethods.TarifMalzemeleriGetir(Convert.ToInt32(row["TarifID"]))
                };

                int toplamMalzemeSayisi = tarif.ToplamMalzemeSayisi;
                tarifListesi.Add(tarif);
            }

            tarifListesi = tarifListesi.OrderByDescending(t => t.ToplamMalzemeSayisi).ToList();

            DataTable siralanmisTarifler = new DataTable();
            siralanmisTarifler.Columns.Add("TarifID", typeof(int));
            siralanmisTarifler.Columns.Add("TarifAdi", typeof(string));
            siralanmisTarifler.Columns.Add("Kategori", typeof(string));
            siralanmisTarifler.Columns.Add("HazirlamaSuresi", typeof(int));
            siralanmisTarifler.Columns.Add("Talimatlar", typeof(string));

            foreach (var tarif in tarifListesi)
            {
                siralanmisTarifler.Rows.Add(tarif.ID, tarif.ad, tarif.kategori, tarif.hazirlanisSuresi, tarif.hazirlamaTalimatlari);
            }

            return siralanmisTarifler;
        }

        public DataTable MalzemeEslesmelerineGoreSirala(DataTable siralanacakTarifler, List<Malzeme> seciliMalzemeler, int minEslesmeYuzdesi)
        {
            DataTable siralanmisTarifler = siralanacakTarifler.Clone();
            siralanmisTarifler.Columns.Add("EslesmeYuzdesi", typeof(decimal));

            foreach (DataRow rowTarif in siralanacakTarifler.Rows)
            {
                int tarifID = Convert.ToInt32(rowTarif["TarifID"]);
                List<Malzeme> tarifMalzemeleri = dBMethods.TarifMalzemeleriGetir(tarifID).Keys.ToList();

                int eslesenMalzemeSayisi = seciliMalzemeler.Count(seciliMalzeme =>
                    tarifMalzemeleri.Any(tarifMalzeme => tarifMalzeme.ad == seciliMalzeme.ad));

                decimal eslesmeYuzdesi = (decimal)eslesenMalzemeSayisi / tarifMalzemeleri.Count * 100;

                DataRow yeniSatir = siralanmisTarifler.NewRow();
                yeniSatir.ItemArray = rowTarif.ItemArray;
                yeniSatir["EslesmeYuzdesi"] = eslesmeYuzdesi;

                if (eslesmeYuzdesi >= minEslesmeYuzdesi && minEslesmeYuzdesi != 0)
                {
                    yeniSatir["TarifAdi"] = rowTarif["TarifAdi"].ToString() + $" - ★ %({eslesmeYuzdesi.ToString("F2")})";
                }
                else
                {
                    yeniSatir["TarifAdi"] = rowTarif["TarifAdi"].ToString() + $" - %({eslesmeYuzdesi.ToString("F2")})";
                }

                siralanmisTarifler.Rows.Add(yeniSatir);
            }

            DataView dataView = siralanmisTarifler.DefaultView;
            dataView.Sort = "EslesmeYuzdesi DESC";
            siralanmisTarifler = dataView.ToTable();

            return siralanmisTarifler;
        }

        public decimal SeciliTarifMalzemeEslesmeYuzdesi(Tarif seciliTarif, List<Malzeme> seciliMalzemeler)
        {
            if (seciliTarif.malzemeler == null || seciliTarif.malzemeler.Count == 0)
            {
                return 0;
            }

            int eslesenMalzemeSayisi = seciliMalzemeler.Count(seciliMalzeme =>
                seciliTarif.malzemeler.Keys.Any(tarifMalzeme => tarifMalzeme.ad == seciliMalzeme.ad));

            decimal eslesmeYuzdesi = (decimal)eslesenMalzemeSayisi / seciliTarif.malzemeler.Count * 100;

            return eslesmeYuzdesi;
        }

        public DataTable TarifYapilabilirMi(DataTable kontrolEdilecekTarifler, DataTable tumMalzemeler, bool tarifYapilabilirMi)
        {
            DataTable filtrelenmisTarifler = kontrolEdilecekTarifler.Clone();
            filtrelenmisTarifler.Columns.Add("EksikMaliyet", typeof(decimal));

            foreach (DataRow rowTarif in kontrolEdilecekTarifler.Rows)
            {
                Tarif tarif = new Tarif
                {
                    ID = Convert.ToInt32(rowTarif["TarifID"]),
                    ad = rowTarif["TarifAdi"].ToString(),
                    kategori = rowTarif["Kategori"].ToString(),
                    hazirlanisSuresi = Convert.ToInt32(rowTarif["HazirlamaSuresi"]),
                    hazirlamaTalimatlari = rowTarif["Talimatlar"].ToString(),
                    malzemeler = dBMethods.TarifMalzemeleriGetir(Convert.ToInt32(rowTarif["TarifID"]))
                };

                bool yapilabilir = true;
                decimal eksikMaliyet = 0;

                foreach (var malzeme in tarif.malzemeler)
                {
                    DataRow[] uygunMalzemeler = tumMalzemeler.Select($"MalzemeAdi = '{malzeme.Key.ad}'");
                    if (uygunMalzemeler.Length == 0 || Convert.ToInt32(uygunMalzemeler[0]["ToplamMiktar"]) < malzeme.Value)
                    {
                        yapilabilir = false;
                        eksikMaliyet += malzeme.Key.birimFiyat * (malzeme.Value - (uygunMalzemeler.Length > 0 ? Convert.ToInt32(uygunMalzemeler[0]["ToplamMiktar"]) : 0));
                    }
                }

                if (tarifYapilabilirMi && yapilabilir)
                {
                    DataRow yeniSatir = filtrelenmisTarifler.NewRow();
                    yeniSatir.ItemArray = rowTarif.ItemArray;
                    yeniSatir["EksikMaliyet"] = 0;
                    filtrelenmisTarifler.Rows.Add(yeniSatir);
                }
                else if (!tarifYapilabilirMi && !yapilabilir)
                {
                    DataRow yeniSatir = filtrelenmisTarifler.NewRow();
                    yeniSatir.ItemArray = rowTarif.ItemArray;
                    yeniSatir["EksikMaliyet"] = eksikMaliyet;
                    filtrelenmisTarifler.Rows.Add(yeniSatir);
                }
            }

            return filtrelenmisTarifler;
        }

        public bool SeciliTarifYapilabilirMi(Tarif seciliTarif, DataTable tumMalzemeler)
        {
            bool yapilabilir = true;

            foreach (var malzeme in seciliTarif.malzemeler)
            {
                DataRow[] uygunMalzemeler = tumMalzemeler.Select($"MalzemeAdi = '{malzeme.Key.ad}'");

                if (uygunMalzemeler.Length == 0 || Convert.ToInt32(uygunMalzemeler[0]["ToplamMiktar"]) < malzeme.Value)
                {
                    yapilabilir = false;
                    break;
                }
            }

            return yapilabilir;
        }

        public int SeciliTarifEksikMaliyet(Tarif seciliTarif, DataTable tumMalzemeler)
        {
            int eksikMaliyet = 0;

            foreach (var malzeme in seciliTarif.malzemeler)
            {
                DataRow[] uygunMalzemeler = tumMalzemeler.Select($"MalzemeAdi = '{malzeme.Key.ad}'");
                int mevcutMiktar = uygunMalzemeler.Length > 0 ? Convert.ToInt32(uygunMalzemeler[0]["ToplamMiktar"]) : 0;

                if (mevcutMiktar < malzeme.Value)
                {
                    int eksikMiktar = malzeme.Value - mevcutMiktar;
                    eksikMaliyet += (int)(malzeme.Key.birimFiyat * eksikMiktar);
                }
            }

            return eksikMaliyet;
        }

        public DataTable MaaliyeteGoreSirala(DataTable siralanacakTarifler, bool _maaliyetBool)
        {
            List<Tarif> tarifListesi = new List<Tarif>();

            foreach (DataRow row in siralanacakTarifler.Rows)
            {
                Tarif tarif = new Tarif
                {
                    ID = Convert.ToInt32(row["TarifID"]),
                    ad = row["TarifAdi"].ToString(),
                    kategori = row["Kategori"].ToString(),
                    hazirlanisSuresi = Convert.ToInt32(row["HazirlamaSuresi"]),
                    hazirlamaTalimatlari = row["Talimatlar"].ToString(),
                    malzemeler = dBMethods.TarifMalzemeleriGetir(Convert.ToInt32(row["TarifID"]))
                };

                decimal maliyet = tarif.Maliyet;
                tarifListesi.Add(tarif);
            }

            if (_maaliyetBool)
            {
                tarifListesi = tarifListesi.OrderBy(t => t.Maliyet).ToList();
            }
            else
            {
                tarifListesi = tarifListesi.OrderByDescending(t => t.Maliyet).ToList();
            }

            DataTable siralanmisTarifler = new DataTable();
            siralanmisTarifler.Columns.Add("TarifID", typeof(int));
            siralanmisTarifler.Columns.Add("TarifAdi", typeof(string));
            siralanmisTarifler.Columns.Add("Kategori", typeof(string));
            siralanmisTarifler.Columns.Add("HazirlamaSuresi", typeof(int));
            siralanmisTarifler.Columns.Add("Talimatlar", typeof(string));

            foreach (var tarif in tarifListesi)
            {
                siralanmisTarifler.Rows.Add(tarif.ID, tarif.ad, tarif.kategori, tarif.hazirlanisSuresi, tarif.hazirlamaTalimatlari);
            }

            return siralanmisTarifler;
        }

        public DataTable MaliyetAraliginaGoreSirala(DataTable siralanacakTarifler, int minMaliyet, int maxMaliyet)
        {
            List<Tarif> tarifListesi = new List<Tarif>();

            foreach (DataRow row in siralanacakTarifler.Rows)
            {
                Tarif tarif = new Tarif
                {
                    ID = Convert.ToInt32(row["TarifID"]),
                    ad = row["TarifAdi"].ToString(),
                    kategori = row["Kategori"].ToString(),
                    hazirlanisSuresi = Convert.ToInt32(row["HazirlamaSuresi"]),
                    hazirlamaTalimatlari = row["Talimatlar"].ToString(),
                    malzemeler = dBMethods.TarifMalzemeleriGetir(Convert.ToInt32(row["TarifID"]))
                };

                if (tarif.Maliyet > minMaliyet && tarif.Maliyet < maxMaliyet)
                    tarifListesi.Add(tarif);
            }

            DataTable siralanmisTarifler = new DataTable();
            siralanmisTarifler.Columns.Add("TarifID", typeof(int));
            siralanmisTarifler.Columns.Add("TarifAdi", typeof(string));
            siralanmisTarifler.Columns.Add("Kategori", typeof(string));
            siralanmisTarifler.Columns.Add("HazirlamaSuresi", typeof(int));
            siralanmisTarifler.Columns.Add("Talimatlar", typeof(string));

            foreach (var tarif in tarifListesi)
            {
                siralanmisTarifler.Rows.Add(tarif.ID, tarif.ad, tarif.kategori, tarif.hazirlanisSuresi, tarif.hazirlamaTalimatlari);
            }

            return siralanmisTarifler;
        }

        public DataTable SureyeGoreSirala(DataTable siralanacakTarifler, bool _sureBool)
        {
            DataView sanalDataTable = siralanacakTarifler.DefaultView;
            if (_sureBool)
            {
                sanalDataTable.Sort = "HazirlamaSuresi ASC";
            }
            else
            {
                sanalDataTable.Sort = "HazirlamaSuresi DESC";
            }
            return sanalDataTable.ToTable();
        }

        public DataTable KategoriyeGoreFiltrele(DataTable siralanacakTarifler, string _kategoriAdi)
        {
            DataView sanalDataTable = siralanacakTarifler.DefaultView;
            sanalDataTable.RowFilter = $"Kategori = '{_kategoriAdi}'";
            return sanalDataTable.ToTable();
        }

        public DataTable TarifAdinaGoreFiltrele(DataTable siralanacakTarifler, string _tarifAdi)
        {
            DataView sanalDataTable = siralanacakTarifler.DefaultView;
            sanalDataTable.RowFilter = $"TarifAdi LIKE '%{_tarifAdi}%'";
            return sanalDataTable.ToTable();
        }

        public bool MalzemeSeciliTarifteYeterliMi(Tarif tarif, Malzeme malzeme)
        {
            foreach (var tarifMalzeme in tarif.malzemeler)
            {
                if (tarifMalzeme.Key.ad == malzeme.ad)
                    if (tarifMalzeme.Value < Convert.ToInt32(malzeme.toplamMiktar))
                        return true;
            }

            return false;
        }

        public DataTable MalzemeAdinaGoreFiltrele(DataTable filtrelenecekMalzemeler, string malzemeAdi)
        {
            DataTable filtrelenmisMalzemeler = filtrelenecekMalzemeler.Clone(); 

            foreach (DataRow row in filtrelenecekMalzemeler.Rows)
            {
                if (row["MalzemeAdi"].ToString().IndexOf(malzemeAdi, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    filtrelenmisMalzemeler.ImportRow(row);
                }
            }

            return filtrelenmisMalzemeler;
        }

    }
}
