using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;

namespace YazLab1
{
    internal class DBMethods
    {
        private AccessDB accessDB = new AccessDB();

        #region TARİFLER
        public bool TarifVarMi(string tarifAdi)
        {
            bool varMi = false;
            using (SqlConnection connection = accessDB.OpenConnection())
            {
                try
                {
                    string query = "SELECT COUNT(1) FROM Tarifler WHERE TarifAdi = @TarifAdi";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@TarifAdi", tarifAdi);
                        int count = Convert.ToInt32(command.ExecuteScalar());
                        varMi = (count > 0);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Hata oluştu: " + ex.Message);
                }
            }
            return varMi;
        }

        public bool TarifEkle(string tarifAdi, string kategori, int hazirlamaSuresi, string talimatlar)
        {
            string query = "INSERT INTO Tarifler (TarifAdi, Kategori, HazirlamaSuresi, Talimatlar) VALUES (@TarifAdi, @Kategori, @HazirlamaSuresi, @Talimatlar)";

            using (SqlConnection connection = accessDB.OpenConnection())
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TarifAdi", tarifAdi);
                    command.Parameters.AddWithValue("@Kategori", kategori);
                    command.Parameters.AddWithValue("@HazirlamaSuresi", hazirlamaSuresi);
                    command.Parameters.AddWithValue("@Talimatlar", talimatlar);

                    int result = command.ExecuteNonQuery();
                    accessDB.CloseConnection(connection);
                    return result > 0;
                }
            }
        }

        public DataTable TarifleriGetir()
        {
            DataTable tarifTablosu = new DataTable();

            using (SqlConnection connection = accessDB.OpenConnection())
            {
                try
                {
                    string query = "SELECT TarifAdi, Kategori, HazirlamaSuresi, Talimatlar FROM Tarifler ORDER BY TarifAdi";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            adapter.Fill(tarifTablosu);
                        }
                    }
                    accessDB.CloseConnection(connection);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Tarifleri getirirken bir hata oluştu: " + ex.Message);
                }
            }
            return tarifTablosu;
        }

        public DataTable TarifleriGetir(List<string> _listelenmisTarifler)
        {
            DataTable listelenmisTarifler = new DataTable();
            string query = "SELECT TarifID, TarifAdi, Kategori, HazirlamaSuresi, Talimatlar FROM Tarifler";

            if (_listelenmisTarifler != null && _listelenmisTarifler.Count > 0)
            {
                query += " WHERE TarifAdi IN (" + string.Join(", ", _listelenmisTarifler.Select((_, i) => $"@TarifAdi{i}")) + ")";
            }

            using (SqlConnection connection = accessDB.OpenConnection())
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    if (_listelenmisTarifler != null && _listelenmisTarifler.Count > 0)
                    {
                        for (int i = 0; i < _listelenmisTarifler.Count; i++)
                        {
                            command.Parameters.AddWithValue($"@TarifAdi{i}", _listelenmisTarifler[i]);
                        }
                    }

                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(listelenmisTarifler);
                    }
                }

                accessDB.CloseConnection(connection);
            }

            return listelenmisTarifler;
        }
        
        public bool TarifMalzemeEkle(int tarifID, int malzemeID, float malzemeMiktar)
        {
            string query = "INSERT INTO TarifMalzeme (TarifID, MalzemeID, MalzemeMiktar) VALUES (@TarifID, @MalzemeID, @MalzemeMiktar)";

            using (SqlConnection connection = accessDB.OpenConnection())
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TarifID", tarifID);
                    command.Parameters.AddWithValue("@MalzemeID", malzemeID);
                    command.Parameters.AddWithValue("@MalzemeMiktar", malzemeMiktar);

                    int result = command.ExecuteNonQuery();
                    accessDB.CloseConnection(connection);
                    return result > 0;
                }
            }
        }

        public bool TarifMalzemeSil(int tarifID, int malzemeID)
        {
            string query = "DELETE FROM TarifMalzeme WHERE TarifID = @TarifID AND MalzemeID = @MalzemeID";

            using (SqlConnection connection = accessDB.OpenConnection())
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TarifID", tarifID);
                    command.Parameters.AddWithValue("@MalzemeID", malzemeID);

                    int result = command.ExecuteNonQuery();
                    accessDB.CloseConnection(connection);
                    return result > 0;
                }
            }
        }

        public bool TarifMalzemeGuncelle(int tarifID, int malzemeID, float malzemeMiktar)
        {
            string query = "UPDATE TarifMalzeme SET MalzemeMiktar = @MalzemeMiktar WHERE TarifID = @TarifID AND MalzemeID = @MalzemeID";

            using (SqlConnection connection = accessDB.OpenConnection())
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TarifID", tarifID);
                    command.Parameters.AddWithValue("@MalzemeID", malzemeID);
                    command.Parameters.AddWithValue("@MalzemeMiktar", malzemeMiktar);

                    int result = command.ExecuteNonQuery();
                    accessDB.CloseConnection(connection);
                    return result > 0;
                }
            }
        }

        public bool TarifMalzemeVarMi(int tarifID, int malzemeID)
        {
            string query = "SELECT COUNT(*) FROM TarifMalzeme WHERE TarifID = @TarifID AND MalzemeID = @MalzemeID";

            using (SqlConnection connection = accessDB.OpenConnection())
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TarifID", tarifID);
                    command.Parameters.AddWithValue("@MalzemeID", malzemeID);

                    int count = (int)command.ExecuteScalar();
                    accessDB.CloseConnection(connection);
                    return count > 0;
                }
            }
        }

        public int TarifIDGetir(string tarifAdi)
        {
            string query = "SELECT TarifID FROM Tarifler WHERE TarifAdi = @TarifAdi";

            using (SqlConnection connection = accessDB.OpenConnection())
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TarifAdi", tarifAdi);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return Convert.ToInt32(reader["TarifID"]);
                        }
                        else
                        {
                            throw new Exception("tarif bulunamadı.");
                        }
                    }
                }
            }
        }

        public Tarif IDTarifGetir(int tarifID)
        {
            string queryTarif = "SELECT * FROM Tarifler WHERE TarifID = @TarifID";

            Tarif tarif = new Tarif();

            using (SqlConnection connection = accessDB.OpenConnection())
            {
                using (SqlCommand command = new SqlCommand(queryTarif, connection))
                {
                    command.Parameters.AddWithValue("@TarifID", tarifID);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            tarif.ID = Convert.ToInt32(reader["TarifID"]);
                            tarif.ad = reader["TarifAdi"].ToString();
                            tarif.kategori = reader["Kategori"].ToString();
                            tarif.hazirlanisSuresi = Convert.ToInt32(reader["HazirlamaSuresi"]);
                            tarif.hazirlamaTalimatlari = reader["Talimatlar"].ToString();
                        }
                        else
                        {
                            throw new Exception("Tarif bulunamadı.");
                        }
                    }
                }
            }

            return tarif;
        }

        public Dictionary<Malzeme, int> TarifMalzemeleriGetir(int tarifID)
        {
            Dictionary<Malzeme, int> malzemeler = new Dictionary<Malzeme, int>();

            string query = "SELECT m.MalzemeID, m.MalzemeAdi, m.BirimFiyat, m.ToplamMiktar, m.MalzemeBirim, tm.MalzemeMiktar " +
                           "FROM TarifMalzeme tm " +
                           "JOIN Malzemeler m ON tm.MalzemeID = m.MalzemeID " +
                           "WHERE tm.TarifID = @TarifID";

            using (SqlConnection connection = accessDB.OpenConnection())
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TarifID", tarifID);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Malzeme malzeme = new Malzeme
                            {
                                ID = Convert.ToInt32(reader["MalzemeID"]),
                                ad = reader["MalzemeAdi"].ToString(),
                                birimFiyat = Convert.ToDecimal(reader["BirimFiyat"]),
                                toplamMiktar = reader["ToplamMiktar"].ToString(),
                                birim = reader["MalzemeBirim"].ToString()
                            };

                            int miktar = Convert.ToInt32(reader["MalzemeMiktar"]);
                            malzemeler.Add(malzeme, miktar);
                        }
                    }
                }
            }

            return malzemeler;
        }

        public bool TarifGuncelle(int tarifID, string tarifAdi, string kategori, int hazirlamaSuresi, string talimatlar)
        {
            string query = "UPDATE Tarifler SET TarifAdi = @TarifAdi, Kategori = @Kategori, HazirlamaSuresi = @HazirlamaSuresi, Talimatlar = @Talimatlar WHERE TarifID = @TarifID";

            using (SqlConnection connection = accessDB.OpenConnection())
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TarifID", tarifID);
                    command.Parameters.AddWithValue("@TarifAdi", tarifAdi);
                    command.Parameters.AddWithValue("@Kategori", kategori);
                    command.Parameters.AddWithValue("@HazirlamaSuresi", hazirlamaSuresi);
                    command.Parameters.AddWithValue("@Talimatlar", talimatlar);

                    int result = command.ExecuteNonQuery();
                    accessDB.CloseConnection(connection);
                    return result > 0;
                }
            }
        }

        public bool TarifSil(int tarifID)
        {
            string query = "DELETE FROM Tarifler WHERE TarifID = @TarifID";

            using (SqlConnection connection = accessDB.OpenConnection())
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TarifID", tarifID);

                    int result = command.ExecuteNonQuery();
                    accessDB.CloseConnection(connection);
                    return result > 0;
                }
            }
        }
        #endregion

        #region MALZEMELER
        public bool MalzemeVarMi(string malzemeAdi)
        {
            bool varMi = false;
            using (SqlConnection connection = accessDB.OpenConnection())
            {
                try
                {
                    string query = "SELECT COUNT(1) FROM Malzemeler WHERE MalzemeAdi = @MalzemeAdi";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@MalzemeAdi", malzemeAdi);
                        int count = Convert.ToInt32(command.ExecuteScalar());
                        varMi = (count > 0);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Hata oluştu: " + ex.Message);
                }
            }
            return varMi;
        }

        public bool MalzemeEkle(string malzemeAdi, string toplamMiktar, string malzemeBirim, decimal birimFiyat)
        {
            string query = "INSERT INTO Malzemeler (MalzemeAdi, ToplamMiktar, MalzemeBirim, BirimFiyat) VALUES (@MalzemeAdi, @ToplamMiktar, @MalzemeBirim, @BirimFiyat)";

            using (SqlConnection connection = accessDB.OpenConnection())
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MalzemeAdi", malzemeAdi);
                    command.Parameters.AddWithValue("@ToplamMiktar", toplamMiktar);
                    command.Parameters.AddWithValue("@MalzemeBirim", malzemeBirim);
                    command.Parameters.AddWithValue("@BirimFiyat", birimFiyat);

                    int result = command.ExecuteNonQuery();
                    accessDB.CloseConnection(connection);
                    return result > 0;
                }
            }
        }

        public DataTable MalzemeleriGetir()
        {
            DataTable malzemeTablosu = new DataTable();

            try
            {
                using (SqlConnection connection = accessDB.OpenConnection())
                {
                    string query = "SELECT MalzemeAdi, ToplamMiktar, MalzemeBirim FROM Malzemeler ORDER BY MalzemeAdi";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            adapter.Fill(malzemeTablosu);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Malzemeleri getirirken bir hata oluştu: " + ex.Message);
            }

            return malzemeTablosu;
        }

        public bool MalzemeSil(int malzemeID)
        {
            string query = "DELETE FROM Malzemeler WHERE MalzemeID = @MalzemeID";

            using (SqlConnection connection = accessDB.OpenConnection())
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MalzemeID", malzemeID);

                    int result = command.ExecuteNonQuery();
                    accessDB.CloseConnection(connection);
                    return result > 0;
                }
            }
        }

        public int MalzemeIDGetir(string malzemeAdi)
        {
            string query = "SELECT MalzemeID FROM Malzemeler WHERE MalzemeAdi = @MalzemeAdi";

            using (SqlConnection connection = accessDB.OpenConnection())
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MalzemeAdi", malzemeAdi);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return Convert.ToInt32(reader["MalzemeID"]);
                        }
                        else
                        {
                            throw new Exception("Malzeme bulunamadı.");
                        }
                    }
                }
            }
        }

        public Malzeme IDMalzemeGetir(int malzemeID)
        {
            string query = "SELECT * FROM Malzemeler WHERE MalzemeID = @MalzemeID";

            using (SqlConnection connection = accessDB.OpenConnection())
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MalzemeID", malzemeID);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Malzeme malzeme = new Malzeme();
                            malzeme.ID = Convert.ToInt32(reader["MalzemeID"]);
                            malzeme.ad = reader["MalzemeAdi"].ToString();
                            malzeme.birimFiyat = Convert.ToDecimal(reader["BirimFiyat"]);
                            malzeme.toplamMiktar = reader["ToplamMiktar"].ToString();
                            malzeme.birim = reader["MalzemeBirim"].ToString();
                            return malzeme;
                        }
                        else
                        {
                            throw new Exception("Malzeme bulunamadı.");
                        }
                    }
                }
            }
        }

        public bool MalzemeGuncelle(int malzemeID, string malzemeAdi, string toplamMiktar, string malzemeBirim, decimal birimFiyat)
        {
            string query = "UPDATE Malzemeler SET MalzemeAdi = @MalzemeAdi, ToplamMiktar = @ToplamMiktar, MalzemeBirim = @MalzemeBirim, BirimFiyat = @BirimFiyat WHERE MalzemeID = @MalzemeID";

            using (SqlConnection connection = accessDB.OpenConnection())
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MalzemeID", malzemeID);
                    command.Parameters.AddWithValue("@MalzemeAdi", malzemeAdi);
                    command.Parameters.AddWithValue("@ToplamMiktar", toplamMiktar);
                    command.Parameters.AddWithValue("@MalzemeBirim", malzemeBirim);
                    command.Parameters.AddWithValue("@BirimFiyat", birimFiyat);

                    int result = command.ExecuteNonQuery();
                    return result > 0;
                }
            }
        }
        #endregion
    }
}
