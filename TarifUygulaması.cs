using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace YazLab1
{
    public partial class AnaForm : Form
    {
        TarifEkle tarifEkle;
        TarifGuncelle tarifGuncelle;
        Tarif seciliTarif;
        DBMethods dbMethods;
        FiltrelemeIslemleri filtrelemeIslemleri;

        public AnaForm()
        {
            InitializeComponent();

            filtrelemeIslemleri = new FiltrelemeIslemleri();
            dbMethods = new DBMethods();

            cmbBxTarifKategoriFiltreleDüzenle();
            cmbBxMalzemeBirimiDüzenle();
            MalzemeleriGetir();
            TarifleriGetir();
        }

        #region TARİFLER

        #region METOTLAR

        private void TarifleriGetir()
        {
            lstBxTarifler.Items.Clear();

            DataTable tarifler = dbMethods.TarifleriGetir();

            foreach (DataRow row in tarifler.Rows)
            {
                string tarifAdi = row["TarifAdi"].ToString();
                lstBxTarifler.Items.Add($"{tarifAdi}");
            }

            lstBxTarifler.DrawMode = DrawMode.OwnerDrawFixed;
            lstBxTarifler.DrawItem += lstBxTarifler_DrawItem;
        }

        public string TarifIsimSadelestir(string _tarifAd)
        {
            string tarifAd = "";

            foreach (char karakter in _tarifAd)
            {
                if (karakter == '-')
                    break;

                tarifAd += karakter;
            }

            tarifAd = tarifAd.Trim();

            return tarifAd;
        }

        private void TarifBilgileriniGetir()
        {
            if (lstBxTarifler.SelectedItem == null)
                return;

            string secilenTarif = lstBxTarifler.SelectedItem.ToString();
            int secilenTarifID = dbMethods.TarifIDGetir(TarifIsimSadelestir(secilenTarif));
            seciliTarif = dbMethods.IDTarifGetir(secilenTarifID);

            txtBxTarifAd.Text = seciliTarif.ad;
            cmbBxTarifKategori.SelectedItem = seciliTarif.kategori;
            nmrcPDwTarifSuresi.Value = seciliTarif.hazirlanisSuresi;
            txtBxTarifYapilis.Text = seciliTarif.hazirlamaTalimatlari;

            seciliTarif.malzemeler = dbMethods.TarifMalzemeleriGetir(secilenTarifID);

            lstBxMalzemeler.Items.Clear();
            foreach (var malzeme in seciliTarif.malzemeler)
            {
                string malzemeBilgisi = $"{malzeme.Key.ad} - {malzeme.Value} {malzeme.Key.birim}";

                int index = lstBxMalzemeler.Items.Add(malzemeBilgisi);
            }

            lstBxMalzemeler.DrawMode = DrawMode.OwnerDrawFixed;
            lstBxMalzemeler.DrawItem += lstBxMalzemeler_DrawItem;
        }

        #endregion

        #region AKSİYONLAR

        private void lstBxTarifler_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            string tarifAd = lstBxTarifler.Items[e.Index].ToString();
            int tarifID = dbMethods.TarifIDGetir(TarifIsimSadelestir(tarifAd));
            Tarif tarif = dbMethods.IDTarifGetir(tarifID);
            tarif.malzemeler = dbMethods.TarifMalzemeleriGetir(tarifID);

            DataTable malzemeler = dbMethods.MalzemeleriGetir();

            bool yapilabilirMi = filtrelemeIslemleri.SeciliTarifYapilabilirMi(tarif, malzemeler);

            Color textColor;
            if (yapilabilirMi)
            {
                textColor = Color.Green;
            }
            else
            {
                textColor = Color.Red;
            }


            e.DrawBackground();
            using (Brush brush = new SolidBrush(textColor))
            {
                e.Graphics.DrawString(tarifAd, e.Font, brush, e.Bounds);
            }
            e.DrawFocusRectangle();
        }

        private void btnTarifiEkle_Click(object sender, EventArgs e)
        {
            tarifEkle = new TarifEkle();
            tarifEkle.ShowDialog();
            TarifleriGetir();
        }

        private void btnTarifiSil_Click(object sender, EventArgs e)
        {
            string secilenTarif = lstBxTarifler.SelectedItem.ToString();
            int secilenTarifID = dbMethods.TarifIDGetir(TarifIsimSadelestir(secilenTarif));
            dbMethods.TarifSil(secilenTarifID);
            TarifleriGetir();

            lstBxMalzemeler.Items.Clear();
            txtBxTarifAd.Clear();
            txtBxTarifYapilis.Clear();
            cmbBxTarifKategori.SelectedIndex = -1;
            nmrcPDwTarifSuresi.Value = 0;
        }

        private void btnTarifiGuncelle_Click(object sender, EventArgs e)
        {
            if (lstBxTarifler.SelectedIndex == -1) return;

            tarifGuncelle = new TarifGuncelle(seciliTarif);
            tarifGuncelle.ShowDialog();
            TarifleriGetir();
        }

        private void lstBxTarifler_SelectedValueChanged(object sender, EventArgs e)
        {
            if (lstBxTarifler.SelectedIndex >= 0)
            {
                btnTarifiGuncelle.Enabled = true;
                btnTarifiSil.Enabled = true;
            }
            else
            {
                btnTarifiGuncelle.Enabled = false;
                btnTarifiSil.Enabled = false;
            }

            TarifBilgileriniGetir();
        }

        private void lstBxTarifler_SelectedIndexChanged(object sender, EventArgs e)
        {
            SeciliTarifEksikMaliyet();
            SeciliTarifMalzemeEslesmeYuzdesi();
            SeciliTarifToplamMaliyet();
        }

        private void SeciliTarifToplamMaliyet()
        {
            if (lstBxTarifler.SelectedItem == null)
            {
                lblEksikMalzemeMaliyet.Text = "Seçim yapılmadı.";
                return;
            }

            string secilenTarifAdi = lstBxTarifler.SelectedItem.ToString();
            int secilenTarifID = dbMethods.TarifIDGetir(TarifIsimSadelestir(secilenTarifAdi));

            Tarif seciliTarif = dbMethods.IDTarifGetir(secilenTarifID);
            seciliTarif.malzemeler = dbMethods.TarifMalzemeleriGetir(secilenTarifID);

            lblSeciliTarifMaliyet.Text = $"Tarifin toplam maliyeti: {seciliTarif.Maliyet} TL";
        }

        private void SeciliTarifMalzemeEslesmeYuzdesi()
        {
            if (lstBxTarifler.SelectedItem == null)
            {
                lblEksikMalzemeMaliyet.Text = "Seçim yapılmadı.";
                return;
            }

            string secilenTarifAdi = lstBxTarifler.SelectedItem.ToString();
            int secilenTarifID = dbMethods.TarifIDGetir(TarifIsimSadelestir(secilenTarifAdi));

            Tarif seciliTarif = dbMethods.IDTarifGetir(secilenTarifID);
            seciliTarif.malzemeler = dbMethods.TarifMalzemeleriGetir(secilenTarifID);

            List<Malzeme> seciliMalzemeler = SecilenMalzemeler();

            decimal eslesmeYuzdesi = filtrelemeIslemleri.SeciliTarifMalzemeEslesmeYuzdesi(seciliTarif, seciliMalzemeler);

            lblMalzemeEslesmeYuzdesi.Text = $"Malzemelere göre eslesme yüzdesi: %{eslesmeYuzdesi.ToString("F2")}";
        }

        private void SeciliTarifEksikMaliyet()
        {
            if (lstBxTarifler.SelectedItem == null)
            {
                lblEksikMalzemeMaliyet.Text = "Seçim yapılmadı.";
                return;
            }

            string secilenTarifAdi = lstBxTarifler.SelectedItem.ToString();
            int secilenTarifID = dbMethods.TarifIDGetir(TarifIsimSadelestir(secilenTarifAdi));

            Tarif seciliTarif = dbMethods.IDTarifGetir(secilenTarifID);
            seciliTarif.malzemeler = dbMethods.TarifMalzemeleriGetir(secilenTarifID);

            DataTable tumMalzemeler = dbMethods.MalzemeleriGetir();

            int eksikMaliyet = filtrelemeIslemleri.SeciliTarifEksikMaliyet(seciliTarif, tumMalzemeler);

            lblEksikMalzemeMaliyet.Text = $"Eksik Malzeme Maliyeti: {eksikMaliyet} TL";
        }

        #endregion

        #endregion

        #region MALZEMELER

        #region METOTLAR

        private void MalzemeleriGetir()
        {
            chckdLstBxTumMalzemeler.Items.Clear();

            DataTable malzemeler = dbMethods.MalzemeleriGetir();

            foreach (DataRow row in malzemeler.Rows)
            {
                string malzemeAdi = row["MalzemeAdi"].ToString();

                chckdLstBxTumMalzemeler.Items.Add($"{malzemeAdi}");
            }
        }

        private void MalzemeBilgileriniGetir()
        {
            string secilenMalzeme = chckdLstBxTumMalzemeler.SelectedItem.ToString();
            int secilenMalzemeID = dbMethods.MalzemeIDGetir(secilenMalzeme);
            Malzeme malzeme = dbMethods.IDMalzemeGetir(secilenMalzemeID);

            txtBxMalzemeAd.Text = malzeme.ad;
            txtBxMalzemeBirimMaaliyet.Text = malzeme.birimFiyat.ToString();
            txtBxMalzemeMiktar.Text = malzeme.toplamMiktar.ToString();
            cmbBxMalzemeBirimi.SelectedItem = malzeme.birim;
        }

        private void MalzemeEkle()
        {
            string malzemeAdi = txtBxMalzemeAd.Text.Trim();
            string toplamMiktar = txtBxMalzemeMiktar.Text.Trim();
            string malzemeBirim = cmbBxMalzemeBirimi.SelectedItem?.ToString();
            decimal birimFiyat;

            if (string.IsNullOrEmpty(malzemeAdi) || string.IsNullOrEmpty(toplamMiktar) || string.IsNullOrEmpty(malzemeBirim) || !decimal.TryParse(txtBxMalzemeBirimMaaliyet.Text.Trim(), out birimFiyat))
            {
                MessageBox.Show("Lütfen tüm alanları doğru bir şekilde doldurun.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (dbMethods.MalzemeVarMi(malzemeAdi))
            {
                MessageBox.Show("Aynı isimde malzeme mevcut", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            bool eklendiMi = dbMethods.MalzemeEkle(malzemeAdi, toplamMiktar, malzemeBirim, birimFiyat);

            if (eklendiMi)
            {
                chckdLstBxTumMalzemeler.Items.Add($"{malzemeAdi}");
                MessageBox.Show("Malzeme başarıyla eklendi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);

                TarifleriGetir();

                txtBxMalzemeAd.Clear();
                txtBxMalzemeBirimMaaliyet.Clear();
                txtBxMalzemeMiktar.Clear();
                cmbBxMalzemeBirimi.SelectedIndex = -1;
            }
            else
                MessageBox.Show("Malzeme eklenemedi. Lütfen tekrar deneyin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void MalzemeSil()
        {
            string secilenMalzeme = chckdLstBxTumMalzemeler.SelectedItem.ToString();
            int secilenMalzemeID = dbMethods.MalzemeIDGetir(secilenMalzeme);
            dbMethods.MalzemeSil(secilenMalzemeID);
            MalzemeleriGetir();
            TarifleriGetir();
        }

        private List<Malzeme> SecilenMalzemeler()
        {
            List<Malzeme> seciliMalzemeler = new List<Malzeme>();

            foreach (string seciliMalzemeAd in chckdLstBxTumMalzemeler.CheckedItems)
            {
                int secilenMalzemeID = dbMethods.MalzemeIDGetir(seciliMalzemeAd);
                Malzeme malzeme = dbMethods.IDMalzemeGetir(secilenMalzemeID);
                seciliMalzemeler.Add(malzeme);
            }

            return seciliMalzemeler;
        }

        private void MalzemeGuncelle()
        {
            string malzemeAdi = txtBxMalzemeAd.Text.Trim();
            string toplamMiktar = txtBxMalzemeMiktar.Text.Trim();
            string malzemeBirim = cmbBxMalzemeBirimi.SelectedItem?.ToString();
            decimal birimFiyat;

            if (string.IsNullOrEmpty(malzemeAdi) || string.IsNullOrEmpty(toplamMiktar) || string.IsNullOrEmpty(malzemeBirim) || !decimal.TryParse(txtBxMalzemeBirimMaaliyet.Text.Trim(), out birimFiyat))
            {
                MessageBox.Show("Lütfen tüm alanları doğru bir şekilde doldurun.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string secilenMalzeme = chckdLstBxTumMalzemeler.SelectedItem.ToString();
            int secilenMalzemeID = dbMethods.MalzemeIDGetir(secilenMalzeme);

            bool guncellendiMi = dbMethods.MalzemeGuncelle(secilenMalzemeID, malzemeAdi, toplamMiktar, malzemeBirim, birimFiyat);

            if (guncellendiMi)
            {
                MalzemeleriGetir();

                MessageBox.Show("Malzeme başarıyla güncellendi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);

                TarifleriGetir();

                txtBxMalzemeAd.Clear();
                txtBxMalzemeBirimMaaliyet.Clear();
                txtBxMalzemeMiktar.Clear();
                cmbBxMalzemeBirimi.SelectedIndex = -1;
            }
            else
                MessageBox.Show("Malzeme güncellenemedi. Lütfen tekrar deneyin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        #endregion

        #region AKSİYONLAR
        private void lstBxMalzemeler_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            Color metinRengi = e.ForeColor;

            string malzemeAdTam = lstBxMalzemeler.Items[e.Index].ToString();
            string malzemeAd = "";

            foreach (char karakter in malzemeAdTam)
            {
                if (karakter == '-')
                    break;

                malzemeAd += karakter;
            }

            malzemeAd = malzemeAd.Trim();
            string malzemeBilgisi = string.Empty;

            string tarifAd = lstBxTarifler.SelectedItem.ToString();
            int tarifID = dbMethods.TarifIDGetir(TarifIsimSadelestir(tarifAd));
            Tarif tarif = dbMethods.IDTarifGetir(tarifID);
            tarif.malzemeler = dbMethods.TarifMalzemeleriGetir(tarifID);

            int malzemeID = dbMethods.MalzemeIDGetir(malzemeAd);
            Malzeme malzeme = dbMethods.IDMalzemeGetir(malzemeID);

            if (dbMethods.TarifMalzemeVarMi(tarifID, malzemeID))
            {
                int malzemeSayiFarki = 0;
                int malzemeninTariftekiSayisi = 0;

                foreach (var tarifMalzeme in tarif.malzemeler)
                {
                    if (tarifMalzeme.Key.ad == malzeme.ad)
                    {
                        malzemeninTariftekiSayisi = tarifMalzeme.Value;
                        malzemeSayiFarki = tarifMalzeme.Value - Convert.ToInt32(malzeme.toplamMiktar);
                    }
                }

                if (filtrelemeIslemleri.MalzemeSeciliTarifteYeterliMi(tarif, malzeme))
                {
                    malzemeBilgisi = $"{malzeme.ad} - {malzemeninTariftekiSayisi} {malzeme.birim}";

                    metinRengi = Color.Green;
                }
                else
                {
                    malzemeBilgisi = $"{malzeme.ad} - {malzemeninTariftekiSayisi} {malzeme.birim} - [{malzemeSayiFarki} {malzeme.birim} eksik]";

                    metinRengi = Color.Red;
                }

                malzemeBilgisi = malzemeBilgisi.Length > 45 ? malzemeBilgisi.Substring(0, 45) + "..." : malzemeBilgisi;
            }

            e.DrawBackground();
            using (Brush brush = new SolidBrush(metinRengi))
            {
                e.Graphics.DrawString(malzemeBilgisi, e.Font, brush, e.Bounds);
            }
            e.DrawFocusRectangle();
        }

        private void btnMalzemeEkle_Click(object sender, EventArgs e)
        {
            MalzemeEkle();
            MalzemeleriGetir();
        }

        private void btnMalzemeSil_Click(object sender, EventArgs e)
        {
            MalzemeSil();
        }

        private void chckdLstBxTumMalzemeler_SelectedValueChanged(object sender, EventArgs e)
        {
            MalzemeBilgileriniGetir();
        }

        private void btnMalzemeGuncelle_Click(object sender, EventArgs e)
        {
            MalzemeGuncelle();
            MalzemeleriGetir();
        }

        #endregion

        #endregion

        #region TARİF FİLTRELEME

        #region METOTLAR

        private bool maaliyetBool;
        private bool tarifSureBool;
        private bool yapilabilirMiBool;

        private DataTable MalzemeFiltrele(DataTable tarifler, List<Malzeme> seciliMalzemeler)
        {
            if (chckBxMalzemelereGoreFiltrele.Checked)
                return filtrelemeIslemleri.MalzemeEslesmelerineGoreSirala(tarifler, seciliMalzemeler, trckBrEslesmeYuzdesi.Value);

            return tarifler;
        }

        private DataTable YapilabilirFiltrele(DataTable tarifler, DataTable malzemeler)
        {
            if (chckBxYapılabilirTarifler.Checked || chckBxYapilamazTarifler.Checked)
            {
                yapilabilirMiBool = chckBxYapılabilirTarifler.Checked;
                return filtrelemeIslemleri.TarifYapilabilirMi(tarifler, malzemeler, yapilabilirMiBool);
            }

            return tarifler;
        }

        private DataTable MaliyetFiltrele(DataTable tarifler)
        {
            if (chckBxMalitetAzalan.Checked || chckBxMaliyetArtan.Checked)
            {
                maaliyetBool = chckBxMaliyetArtan.Checked;
                return filtrelemeIslemleri.MaaliyeteGoreSirala(tarifler, maaliyetBool);
            }

            return tarifler;
        }

        private DataTable SureFiltrele(DataTable tarifler)
        {
            if (chckBxSureAzalan.Checked || chckBxSureArtan.Checked)
            {
                tarifSureBool = chckBxSureArtan.Checked;
                return filtrelemeIslemleri.SureyeGoreSirala(tarifler, tarifSureBool);
            }

            return tarifler;
        }

        private DataTable KategoriFiltrele(DataTable tarifler)
        {
            if (cmbBxTarifKategoriFiltrele.SelectedIndex > 0)
                return filtrelemeIslemleri.KategoriyeGoreFiltrele(tarifler, cmbBxTarifKategoriFiltrele.Text);

            return tarifler;
        }

        private DataTable TarifAdiFiltrele(DataTable tarifler)
        {
            if (txtBxTarifAra.ForeColor == SystemColors.MenuText)
                return filtrelemeIslemleri.TarifAdinaGoreFiltrele(tarifler, txtBxTarifAra.Text);

            return tarifler;
        }

        private DataTable MalzemeSayisiFiltrele(DataTable tarifler)
        {
            if (chckBxMalzemeSayisinaGoreFiltrele.Checked)
                return filtrelemeIslemleri.MalzemeSayisinaGoreSirala(tarifler);

            return tarifler;
        }

        private DataTable MaliyetAraliginaGoreFiltrele(DataTable tarifler)
        {
            return filtrelemeIslemleri.MaliyetAraliginaGoreSirala(tarifler, trckBrMinMaliyet.Value, trckBrMaxMaliyet.Value);
        }

        private void FiltrelenmisTarifListesiniDoldur(DataTable tarifler)
        {
            foreach (DataRow row in tarifler.Rows)
            {
                string tarifAdi = row["TarifAdi"].ToString();
                lstBxTarifler.Items.Add(tarifAdi);
            }
        }

        private void trackBar_Scroll(object sender, EventArgs e)
        {
            TrackBar trackBar = (TrackBar)sender;

            if (trckBrMinMaliyet.Value > trckBrMaxMaliyet.Value)
            {
                if (trackBar == trckBrMinMaliyet)
                {
                    trckBrMinMaliyet.Value = trckBrMaxMaliyet.Value;
                }
                else
                {
                    trckBrMaxMaliyet.Value = trckBrMinMaliyet.Value;
                }
            }

            lblMinMaliyet.Text = $"MİN: {trckBrMinMaliyet.Value} TL";
            lblMaxMaliyet.Text = $"MAX: {trckBrMaxMaliyet.Value} TL";
        }

        #endregion

        #region AKSİYONLAR

        private void btnTarifAra_Click(object sender, EventArgs e)
        {
            lstBxTarifler.Items.Clear();

            DataTable filtrelenecekTarifler = dbMethods.TarifleriGetir(lstBxTarifler.Items.Cast<string>().ToList());
            DataTable malzemeler = dbMethods.MalzemeleriGetir();
            List<Malzeme> seciliMalzemeler = SecilenMalzemeler();

            filtrelenecekTarifler = MaliyetAraliginaGoreFiltrele(filtrelenecekTarifler);
            filtrelenecekTarifler = MalzemeSayisiFiltrele(filtrelenecekTarifler);
            filtrelenecekTarifler = YapilabilirFiltrele(filtrelenecekTarifler, malzemeler);
            filtrelenecekTarifler = MaliyetFiltrele(filtrelenecekTarifler);
            filtrelenecekTarifler = SureFiltrele(filtrelenecekTarifler);
            filtrelenecekTarifler = KategoriFiltrele(filtrelenecekTarifler);
            filtrelenecekTarifler = TarifAdiFiltrele(filtrelenecekTarifler);
            filtrelenecekTarifler = MalzemeFiltrele(filtrelenecekTarifler, seciliMalzemeler);

            FiltrelenmisTarifListesiniDoldur(filtrelenecekTarifler);
        }

        private void trckBrMinMaliyet_Scroll(object sender, EventArgs e)
        {
            trackBar_Scroll(sender, e);
        }

        private void trckBrMaxMaliyet_Scroll(object sender, EventArgs e)
        {
            trackBar_Scroll(sender, e);
        }

        private void trckBrEslesmeYuzdesi_Scroll(object sender, EventArgs e)
        {
            lblEslesmeYuzdesi.Text = "Min Eşleşme: %" + trckBrEslesmeYuzdesi.Value.ToString();
        }

        private void chckBxSureAzalan_CheckedChanged(object sender, EventArgs e)
        {
            if (chckBxSureAzalan.Checked && chckBxSureArtan.Checked)
                chckBxSureArtan.CheckState = CheckState.Unchecked;
        }

        private void chckBxSureArtan_CheckedChanged(object sender, EventArgs e)
        {
            if (chckBxSureAzalan.Checked && chckBxSureArtan.Checked)
                chckBxSureAzalan.CheckState = CheckState.Unchecked;
        }

        private void chckBxYapılabilirTarifler_CheckedChanged(object sender, EventArgs e)
        {
            if (chckBxYapılabilirTarifler.Checked && chckBxYapilamazTarifler.Checked)
                chckBxYapilamazTarifler.CheckState = CheckState.Unchecked;
        }

        private void chckBxYapilamazTarifler_CheckedChanged(object sender, EventArgs e)
        {
            if (chckBxYapılabilirTarifler.Checked && chckBxYapilamazTarifler.Checked)
                chckBxYapılabilirTarifler.CheckState = CheckState.Unchecked;
        }

        private void chckBxMalitetAzalan_CheckedChanged(object sender, EventArgs e)
        {
            if (chckBxMalitetAzalan.Checked && chckBxMaliyetArtan.Checked)
                chckBxMaliyetArtan.CheckState = CheckState.Unchecked;
        }

        private void chckBxMaliyetArtan_CheckedChanged(object sender, EventArgs e)
        {
            if (chckBxMalitetAzalan.Checked && chckBxMaliyetArtan.Checked)
                chckBxMalitetAzalan.CheckState = CheckState.Unchecked;
        }


        #endregion

        #endregion

        #region MALZEME FİLTRELEME

        private void FiltrelenmisMalzemeListesiniDoldur(DataTable malzemeler)
        {
            chckdLstBxTumMalzemeler.Items.Clear();

            foreach (DataRow row in malzemeler.Rows)
            {
                string malzemeAdi = row["MalzemeAdi"].ToString();
                chckdLstBxTumMalzemeler.Items.Add(malzemeAdi);
            }
        }

        private void btnMalzemeFiltrele_Click(object sender, EventArgs e)
        {
            DataTable filtrelenecekMalzemeler = dbMethods.MalzemeleriGetir();

            string malzemeAdi = txtBxMalzemeAd.Text;

            if (txtBxMalzemeAd.ForeColor == SystemColors.MenuText || string.IsNullOrEmpty(txtBxMalzemeAd.Text))
                filtrelenecekMalzemeler = filtrelemeIslemleri.MalzemeAdinaGoreFiltrele(filtrelenecekMalzemeler, malzemeAdi);

            FiltrelenmisMalzemeListesiniDoldur(filtrelenecekMalzemeler);
        }

        #endregion

        #region GUI METOT VE AKSİYONLARI

        private void txtBxTarifAra_Enter(object sender, EventArgs e)
        {
            txtBxTarifAra.ForeColor = SystemColors.MenuText;
            if (txtBxTarifAra.Text == $"Tarifin adı")
                txtBxTarifAra.Text = "";
        }
        private void txtBxTarifAra_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtBxTarifAra.Text))
            {
                txtBxTarifAra.ForeColor = SystemColors.ScrollBar;
                txtBxTarifAra.Text = $"Tarifin adı";
            }
        }

        private void txtBxMalzemeAd_Enter(object sender, EventArgs e)
        {
            txtBxMalzemeAd.ForeColor = SystemColors.MenuText;
            if (txtBxMalzemeAd.Text == "Malzeme adı")
                txtBxMalzemeAd.Text = "";
        }
        private void txtBxMalzemeAd_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtBxMalzemeAd.Text))
            {
                txtBxMalzemeAd.ForeColor = SystemColors.ScrollBar;
                txtBxMalzemeAd.Text = $"Malzeme adı";
            }
        }

        private void txtBxMalzemeBirimMaaliyet_Enter(object sender, EventArgs e)
        {
            txtBxMalzemeBirimMaaliyet.ForeColor = SystemColors.MenuText;
            if (txtBxMalzemeBirimMaaliyet.Text == "Birim maliyet")
                txtBxMalzemeBirimMaaliyet.Text = "";
        }
        private void txtBxMalzemeBirimMaaliyet_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtBxMalzemeBirimMaaliyet.Text))
            {
                txtBxMalzemeBirimMaaliyet.ForeColor = SystemColors.ScrollBar;
                txtBxMalzemeBirimMaaliyet.Text = $"Birim maliyet";
            }
        }

        private void txtBxMalzemeMiktar_Enter(object sender, EventArgs e)
        {
            txtBxMalzemeMiktar.ForeColor = SystemColors.MenuText;
            if (txtBxMalzemeMiktar.Text == "Toplam adet")
                txtBxMalzemeMiktar.Text = "";
        }
        private void txtBxMalzemeMiktar_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtBxMalzemeMiktar.Text))
            {
                txtBxMalzemeMiktar.ForeColor = SystemColors.ScrollBar;
                txtBxMalzemeMiktar.Text = $"Toplam adet";
            }
        }

        private void cmbBxTarifKategoriFiltreleDüzenle()
        {
            cmbBxTarifKategoriFiltrele.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbBxTarifKategoriFiltrele.SelectedIndex = 0;
        }
        private void cmbBxMalzemeBirimiDüzenle()
        {
            cmbBxMalzemeBirimi.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbBxMalzemeBirimi.SelectedIndex = 0;
        }

        private void btnFiltrelemeSifirla_Click(object sender, EventArgs e)
        {
            chckBxMalitetAzalan.Checked = false;
            chckBxMaliyetArtan.Checked = false;
            chckBxSureArtan.Checked = false;
            chckBxSureAzalan.Checked = false;
            chckBxYapilamazTarifler.Checked = false;
            chckBxYapılabilirTarifler.Checked = false;
            chckBxMalzemelereGoreFiltrele.Checked = false;
            chckBxMalzemeSayisinaGoreFiltrele.Checked = false;
            trckBrEslesmeYuzdesi.Value = 0;
            trckBrMaxMaliyet.Value = 4000;
            trckBrMinMaliyet.Value = 0;
            txtBxTarifAra.ForeColor = SystemColors.ScrollBar;
            txtBxTarifAra.Text = "Tarifin adı";
            cmbBxTarifKategoriFiltrele.SelectedIndex = 0;
        }
        private void btnMalzemelerSifirla_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < chckdLstBxTumMalzemeler.Items.Count; i++)
            {
                chckdLstBxTumMalzemeler.SetItemChecked(i, false);
            }

            txtBxMalzemeAd.ForeColor = SystemColors.ScrollBar;
            txtBxMalzemeAd.Text = "Malzeme adı";
            txtBxMalzemeMiktar.ForeColor = SystemColors.ScrollBar;
            txtBxMalzemeMiktar.Text = "Toplam adet";
            txtBxMalzemeBirimMaaliyet.ForeColor = SystemColors.ScrollBar;
            txtBxMalzemeBirimMaaliyet.Text = "Birim maliyet";
            cmbBxMalzemeBirimi.SelectedIndex = -1;
        }

        #endregion

    }
}
