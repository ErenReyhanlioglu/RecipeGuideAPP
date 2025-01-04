using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace YazLab1
{
    public partial class TarifGuncelle : Form
    {
        DBMethods dbMethods = null;
        Tarif guncellenecekTarif = null;
        List<Malzeme> silinecekMalzemeler = null;
        Dictionary<string, int> eklenecekMalzemeler = null;
        FiltrelemeIslemleri filtrelemeIslemleri;

        public TarifGuncelle(Tarif _guncellenecekTarif)
        {
            InitializeComponent();

            silinecekMalzemeler = new List<Malzeme>();
            eklenecekMalzemeler = new Dictionary<string, int>();
            guncellenecekTarif = _guncellenecekTarif;
            dbMethods = new DBMethods();
            filtrelemeIslemleri = new FiltrelemeIslemleri();

            cmbBxTarifKategoriDüzenle();
            TarifBilgileriniGetir();
        }

        #region METOTLAR
        private void TarifBilgileriniGetir()
        {
            if (guncellenecekTarif == null)
                return;

            txtBxTarifAd.Text = guncellenecekTarif.ad;
            cmbBxTarifKategori.SelectedItem = guncellenecekTarif.kategori;
            nmrcPDwTarifSuresi.Value = guncellenecekTarif.hazirlanisSuresi;
            txtBxTarifYapilis.Text = guncellenecekTarif.hazirlamaTalimatlari;

            chckdLstBxMalzemeler.Items.Clear();
            DataTable malzemelerTablosu = dbMethods.MalzemeleriGetir();

            foreach (DataRow row in malzemelerTablosu.Rows)
            {
                string malzemeBilgisi = row["MalzemeAdi"].ToString();

                int index = chckdLstBxMalzemeler.Items.Add(malzemeBilgisi);

                bool isChecked = guncellenecekTarif.malzemeler.Any(m => m.Key.ad == malzemeBilgisi);

                chckdLstBxMalzemeler.SetItemChecked(index, isChecked);
            }
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

        #endregion

        #region AKSİYONLAR
        private void btnTarifGuncelle_Click(object sender, EventArgs e)
        {
            if (guncellenecekTarif == null)
                return;

            string secilenTarif = guncellenecekTarif.ad.ToString();
            secilenTarif = TarifIsimSadelestir(secilenTarif);
            int secilenTarifID = dbMethods.TarifIDGetir(secilenTarif);
            string tarifAdi = txtBxTarifAd.Text.Trim();
            string kategori = cmbBxTarifKategori.SelectedItem?.ToString();
            int hazirlanisSuresi = (int)nmrcPDwTarifSuresi.Value;
            string talimatlar = txtBxTarifYapilis.Text.Trim();

            bool tarifGuncellendiMi = dbMethods.TarifGuncelle(secilenTarifID, tarifAdi, kategori, hazirlanisSuresi, talimatlar);

            if (tarifGuncellendiMi)
            {
                foreach (var malzeme in guncellenecekTarif.malzemeler)
                {
                    int malzemeID = dbMethods.MalzemeIDGetir(malzeme.Key.ad);
                    float malzemeMiktar = malzeme.Value;

                    bool malzemeGuncellendiMi = dbMethods.TarifMalzemeGuncelle(secilenTarifID, malzemeID, malzemeMiktar);
                    if (!malzemeGuncellendiMi)
                    {
                        MessageBox.Show($"Malzeme güncellenirken hata oluştu: {malzeme.Key.ad}");
                    }
                }

                foreach (Malzeme silinecekMalzeme in silinecekMalzemeler)
                {
                    int silinecekMalzemeID = dbMethods.MalzemeIDGetir(silinecekMalzeme.ad);

                    dbMethods.TarifMalzemeSil(secilenTarifID, silinecekMalzemeID);
                }

                foreach (var malzemeD in eklenecekMalzemeler)
                {
                    int malzemeID = dbMethods.MalzemeIDGetir(malzemeD.Key);
                    int miktar = malzemeD.Value;

                    if (dbMethods.TarifMalzemeVarMi(secilenTarifID, malzemeID))
                        continue;

                    dbMethods.TarifMalzemeEkle(secilenTarifID, malzemeID, miktar);
                }

                MessageBox.Show("Tarif bilgileri başarıyla güncellendi.");
                this.Close();
            }
            else
            {
                MessageBox.Show("Tarif bilgileri güncellenirken bir hata meydana geldi.");
            }
        }

        private void chckdLstBxMalzemeler_SelectedValueChanged(object sender, EventArgs e)
        {
            if (chckdLstBxMalzemeler.SelectedIndex == -1)
                return;

            string secilenMalzemeAdi = chckdLstBxMalzemeler.SelectedItem.ToString();
            int secilenMalzemeID = dbMethods.MalzemeIDGetir(secilenMalzemeAdi);
            Malzeme secilenMalzeme = dbMethods.IDMalzemeGetir(secilenMalzemeID);

            if (secilenMalzeme == null)
                return;

            int selectedIndex = chckdLstBxMalzemeler.SelectedIndex;
            bool isChecked = chckdLstBxMalzemeler.GetItemChecked(selectedIndex);

            if (isChecked)
            {
                nmrcPDwnMalzemeMiktari.Enabled = true;
                lblTarifMalzemeSayisi.Text = $"{secilenMalzeme.ad} ({secilenMalzeme.birim}): ";

            }
            else
            {
                nmrcPDwnMalzemeMiktari.Enabled = false;
                lblTarifMalzemeSayisi.Text = $"Seçili malzemeyi aktifleştirin: ";
            }
        }

        private void chckdLstBxMalzemeler_ItemCheck(object sender, ItemCheckEventArgs e) 
        {
            string secilenMalzemeAdi = chckdLstBxMalzemeler.Items[e.Index].ToString();
            int secilenMalzemeID = dbMethods.MalzemeIDGetir(secilenMalzemeAdi);
            Malzeme secilenMalzeme = dbMethods.IDMalzemeGetir(secilenMalzemeID);

            if (secilenMalzeme == null || chckdLstBxMalzemeler.SelectedIndex == -1)
                return;

            int selectedIndex = chckdLstBxMalzemeler.SelectedIndex;
            bool isChecked = chckdLstBxMalzemeler.GetItemChecked(selectedIndex);

            if (isChecked)
                nmrcPDwnMalzemeMiktari.Enabled = true;
            else
                nmrcPDwnMalzemeMiktari.Enabled = false;

            if (e.NewValue == CheckState.Unchecked)
            {
                if (guncellenecekTarif.malzemeler.Keys.Any(x => x.ad == secilenMalzeme.ad) && !silinecekMalzemeler.Any(x => x.ad == secilenMalzeme.ad))
                {
                    silinecekMalzemeler.Add(secilenMalzeme);
                }
                if (eklenecekMalzemeler.Keys.Any(x => x == secilenMalzeme.ad))
                {
                    eklenecekMalzemeler.Remove(secilenMalzeme.ad);
                }
            }

            if (e.NewValue == CheckState.Checked)
            {
                if (!eklenecekMalzemeler.Keys.Any(x => x == secilenMalzeme.ad) && !guncellenecekTarif.malzemeler.Keys.Any(x => x.ad == secilenMalzeme.ad))
                {
                    silinecekMalzemeler.Remove(secilenMalzeme);
                    eklenecekMalzemeler.Add(secilenMalzeme.ad, (int)nmrcPDwnMalzemeMiktari.Value);
                }
                if (silinecekMalzemeler.Any(x => x.ad == secilenMalzeme.ad))
                {
                    silinecekMalzemeler.Remove(secilenMalzeme);
                }
            }

            chckdLstBxMalzemeler.SelectedIndex = selectedIndex;
        }

        private void btnTarifMalzemeSayiKayit_Click(object sender, EventArgs e)
        {
            if (chckdLstBxMalzemeler.SelectedItem == null)
                return;

            int mevcutTopIndex = chckdLstBxMalzemeler.TopIndex;

            string secilenMalzemeAdi = chckdLstBxMalzemeler.SelectedItem.ToString();
            int secilenMalzemeID = dbMethods.MalzemeIDGetir(secilenMalzemeAdi);
            Malzeme secilenMalzeme = dbMethods.IDMalzemeGetir(secilenMalzemeID);

            if ((int)nmrcPDwnMalzemeMiktari.Value == 0 && !silinecekMalzemeler.Any(x => x.ad == secilenMalzeme.ad))
            {
                silinecekMalzemeler.Add(secilenMalzeme);
                chckdLstBxMalzemeler.TopIndex = mevcutTopIndex;
                return;
            }

            if (secilenMalzeme != null && guncellenecekTarif.malzemeler.Keys.Any(x => x.ad == secilenMalzeme.ad))
            {
                int miktar = (int)nmrcPDwnMalzemeMiktari.Value;
                guncellenecekTarif.malzemeler[secilenMalzeme] = miktar;
            }

            if (secilenMalzeme != null && eklenecekMalzemeler.Keys.Any(x => x == secilenMalzeme.ad))
            {
                int miktar = (int)nmrcPDwnMalzemeMiktari.Value;
                eklenecekMalzemeler[secilenMalzeme.ad] = miktar;
            }

            chckdLstBxMalzemeler.TopIndex = mevcutTopIndex;
        }
        #endregion

        #region GUI METOT VE AKSİYONLARI
        private void cmbBxTarifKategoriDüzenle()
        {
            cmbBxTarifKategori.DropDownStyle = ComboBoxStyle.DropDownList;
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

        private void FiltrelenmisMalzemeListesiniDoldur(DataTable malzemeler)
        {
            chckdLstBxMalzemeler.Items.Clear();

            foreach (DataRow row in malzemeler.Rows)
            {
                string malzemeBilgisi = row["MalzemeAdi"].ToString();

                int index = chckdLstBxMalzemeler.Items.Add(malzemeBilgisi);

                bool isChecked = guncellenecekTarif.malzemeler.Any(m => m.Key.ad == malzemeBilgisi);

                chckdLstBxMalzemeler.SetItemChecked(index, isChecked);
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
