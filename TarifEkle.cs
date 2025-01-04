using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace YazLab1
{
    public partial class TarifEkle : Form
    {
        DBMethods dbMethods;
        Tarif yeniTarif;
        FiltrelemeIslemleri filtrelemeIslemleri;

        public TarifEkle()
        {
            InitializeComponent();

            dbMethods = new DBMethods();
            yeniTarif = new Tarif();
            filtrelemeIslemleri = new FiltrelemeIslemleri();

            cmbBxTarifKategoriFiltreleDüzenle();
            TariflerIcinMalzemeleriGetir();
        }

        #region METOTLAR
        private void TariflerIcinMalzemeleriGetir(string kaldirilacakMalzemeAd = null)
        {
            chckdLstBxMalzemeler.Items.Clear();

            DataTable malzemeler = dbMethods.MalzemeleriGetir();

            foreach (DataRow row in malzemeler.Rows)
            {
                string malzemeBilgisi = MalzemeIsimSadelestir(row["MalzemeAdi"].ToString());
                bool checkedDurumu = false;

                foreach (var yeniMalzeme in yeniTarif.malzemeler.Keys)
                {
                    if (malzemeBilgisi == MalzemeIsimSadelestir(yeniMalzeme.ad.ToString()) && yeniTarif.malzemeler[yeniMalzeme] != 0)
                    {
                        if(malzemeBilgisi == kaldirilacakMalzemeAd && kaldirilacakMalzemeAd != null)
                            continue;

                        malzemeBilgisi = $"{yeniMalzeme.ad} - {yeniTarif.malzemeler[yeniMalzeme]} {yeniMalzeme.birim}";
                        checkedDurumu = true;
                        continue;
                    }
                }

                chckdLstBxMalzemeler.Items.Add(malzemeBilgisi, checkedDurumu);
            }
        }

        private string TarifIsimSadelestir(string _tarifAd)
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

        private string MalzemeIsimSadelestir(string _malzemeAd)
        {
            string malzemeAd = "";

            foreach (char karakter in _malzemeAd)
            {
                if (karakter == '-')
                    break;

                malzemeAd += karakter;
            }

            malzemeAd = malzemeAd.Trim();

            return malzemeAd;
        }
        #endregion

        #region AKSİYONLAR
        private void btnTarifiEkle_Click(object sender, EventArgs e)
        {
            string tarifAdi = txtBxTarifAd.Text.Trim();
            string kategori = cmbBxTarifKategori.SelectedItem?.ToString();
            string hazirlamaTalimatlari = txtBxTarifYapilis.Text.Trim();
            int hazirlanisSuresi;

            if (string.IsNullOrEmpty(tarifAdi) || string.IsNullOrEmpty(kategori) || string.IsNullOrEmpty(hazirlamaTalimatlari) || !int.TryParse(nmrcPDwTarifSuresi.Value.ToString(), out hazirlanisSuresi))
            {
                MessageBox.Show("Lütfen tüm alanları doğru bir şekilde doldurun.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (dbMethods.TarifVarMi(tarifAdi))
            {
                MessageBox.Show("Aynı isimde tarif mevcut.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            bool tarifEklendiMi = dbMethods.TarifEkle(tarifAdi, kategori, hazirlanisSuresi, hazirlamaTalimatlari);

            if (tarifEklendiMi)
            {
                tarifAdi = TarifIsimSadelestir(tarifAdi);
                int yeniTarifID = dbMethods.TarifIDGetir(tarifAdi);

                foreach (var malzemeD in yeniTarif.malzemeler)
                {
                    Malzeme malzeme = malzemeD.Key;
                    int malzemeMiktar = malzemeD.Value;

                    if (dbMethods.TarifMalzemeVarMi(yeniTarifID, malzeme.ID))
                        continue;

                    dbMethods.TarifMalzemeEkle(yeniTarifID, malzeme.ID, malzemeMiktar);
                }

                MessageBox.Show("Tarif başarıyla eklendi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            else
                MessageBox.Show("Tarif eklenemedi. Lütfen tekrar deneyin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void chckdLstBxMalzemeler_SelectedValueChanged(object sender, EventArgs e)
        {
            if (chckdLstBxMalzemeler.SelectedIndex == -1)
                return;

            int selectedIndex = chckdLstBxMalzemeler.SelectedIndex;
            bool isChecked = chckdLstBxMalzemeler.GetItemChecked(selectedIndex);

            string secilenMalzemeAdi = chckdLstBxMalzemeler.SelectedItem.ToString();
            secilenMalzemeAdi = MalzemeIsimSadelestir(secilenMalzemeAdi);
            int secilenMalzemeID = dbMethods.MalzemeIDGetir(secilenMalzemeAdi);
            Malzeme secilenMalzeme = dbMethods.IDMalzemeGetir(secilenMalzemeID);

            if (secilenMalzeme == null)
                return;

            if (isChecked)
            {
                nmrcPDwnMalzemeMiktari.Enabled = true;
                nmrcPDwnMalzemeMiktari.Value = 0;
                lblTarifMalzemeSayisi.Text = $"{secilenMalzeme.ad} ({secilenMalzeme.birim}): ";
            }
            else
            {
                nmrcPDwnMalzemeMiktari.Enabled = false;
                nmrcPDwnMalzemeMiktari.Value = 0;
                lblTarifMalzemeSayisi.Text = $"Seçili malzemeyi aktifleştirin: ";
            }
        }

        private void chckdLstBxMalzemeler_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            string secilenMalzemeAdi = chckdLstBxMalzemeler.Items[e.Index].ToString();
            secilenMalzemeAdi = MalzemeIsimSadelestir(secilenMalzemeAdi);
            int secilenMalzemeID = dbMethods.MalzemeIDGetir(secilenMalzemeAdi);
            Malzeme secilenMalzeme = dbMethods.IDMalzemeGetir(secilenMalzemeID);

            if (secilenMalzeme == null || e.Index == -1)
                return;

            bool isChecked = chckdLstBxMalzemeler.GetItemChecked(e.Index);

            nmrcPDwnMalzemeMiktari.Enabled = isChecked;
            nmrcPDwnMalzemeMiktari.Value = 0;

            if (e.NewValue == CheckState.Unchecked) 
            {
                Malzeme kaldirilacakMalzeme = null;

                foreach (var yeniMalzeme in yeniTarif.malzemeler.Keys)
                {
                    if (yeniMalzeme.ad == secilenMalzeme.ad)
                    {
                        kaldirilacakMalzeme = yeniMalzeme;
                        break; 
                    }
                }

                if (kaldirilacakMalzeme != null)
                {
                    yeniTarif.malzemeler.Remove(kaldirilacakMalzeme);
                    TariflerIcinMalzemeleriGetir(kaldirilacakMalzeme.ad);
                }
            }
        }


        private void btnTarifMalzemeSayiKayit_Click(object sender, EventArgs e)
        {
            if (chckdLstBxMalzemeler.SelectedItem == null)
                return;

            int mevcutTopIndex = chckdLstBxMalzemeler.TopIndex;

            string secilenMalzemeAdi = chckdLstBxMalzemeler.SelectedItem.ToString();
            secilenMalzemeAdi = MalzemeIsimSadelestir(secilenMalzemeAdi);
            int secilenMalzemeID = dbMethods.MalzemeIDGetir(secilenMalzemeAdi);
            Malzeme secilenMalzeme = dbMethods.IDMalzemeGetir(secilenMalzemeID);

            int miktar = (int)nmrcPDwnMalzemeMiktari.Value;
            yeniTarif.malzemeler[secilenMalzeme] = miktar;
            
            TariflerIcinMalzemeleriGetir();

            chckdLstBxMalzemeler.TopIndex = mevcutTopIndex;
        }
        #endregion

        #region GUI METOT VE AKSİYONLARI
        private void cmbBxTarifKategoriFiltreleDüzenle()
        {
            cmbBxTarifKategori.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbBxTarifKategori.SelectedIndex = 0;
        }

        private void txtBxTarifAd_Enter(object sender, EventArgs e)
        {
            txtBxTarifAd.ForeColor = SystemColors.MenuText;
            if (txtBxTarifAd.Text == $"Tarif adı")
                txtBxTarifAd.Text = "";
        }

        private void txtBxTarifAd_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtBxTarifAd.Text))
            {
                txtBxTarifAd.ForeColor = SystemColors.ScrollBar;
                txtBxTarifAd.Text = $"Tarif adı";
            }
        }

        private void txtBxTarifYapilis_Enter(object sender, EventArgs e)
        {
            txtBxTarifYapilis.ForeColor = SystemColors.MenuText;
            if (txtBxTarifYapilis.Text == $"Tarif yapılışı")
                txtBxTarifYapilis.Text = "";
        }

        private void txtBxTarifYapilis_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtBxTarifYapilis.Text))
            {
                txtBxTarifYapilis.ForeColor = SystemColors.ScrollBar;
                txtBxTarifYapilis.Text = $"Tarif yapılışı";
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
        #endregion

        #region MALZEME FİLTRELEME

        private void FiltrelenmisMalzemeListesiniDoldur(DataTable malzemeler, string kaldirilacakMalzemeAd = null)
        {
            chckdLstBxMalzemeler.Items.Clear();

            foreach (DataRow row in malzemeler.Rows)
            {
                string malzemeBilgisi = MalzemeIsimSadelestir(row["MalzemeAdi"].ToString());
                bool checkedDurumu = false;

                foreach (var yeniMalzeme in yeniTarif.malzemeler.Keys)
                {
                    if (malzemeBilgisi == MalzemeIsimSadelestir(yeniMalzeme.ad.ToString()) && yeniTarif.malzemeler[yeniMalzeme] != 0)
                    {
                        if (malzemeBilgisi == kaldirilacakMalzemeAd && kaldirilacakMalzemeAd != null)
                            continue;

                        malzemeBilgisi = $"{yeniMalzeme.ad} - {yeniTarif.malzemeler[yeniMalzeme]} {yeniMalzeme.birim}";
                        checkedDurumu = true;
                        continue;
                    }
                }

                chckdLstBxMalzemeler.Items.Add(malzemeBilgisi, checkedDurumu);
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
    }
}
