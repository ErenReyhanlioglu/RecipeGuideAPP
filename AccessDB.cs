using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;


namespace YazLab1
{
    internal class AccessDB
    {
        private string connectionString = "Server=Eren\\SQLEXPRESS;Database=YazLab1DB;User Id=sa;Password=sa123;";

        public SqlConnection OpenConnection()
        {
            SqlConnection connection = new SqlConnection(connectionString);
            try
            {
                connection.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veritabanı bağlantısı açılamadı: " + ex.Message); 
            }
            return connection;
        }

        public void CloseConnection(SqlConnection connection)
        {
            try
            {
                if (connection != null && connection.State == System.Data.ConnectionState.Open)
                {
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veritabanı bağlantısı kapatılamadı: " + ex.Message);
            }
        }
    }
}
