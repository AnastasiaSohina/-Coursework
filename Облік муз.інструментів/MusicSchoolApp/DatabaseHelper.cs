using System;
using System.Data;
using MySql.Data.MySqlClient;
using System.Windows.Forms;

namespace MusicSchoolApp
{
    public class DatabaseHelper
    {
        private static string connectionString = "Server=localhost;Database=music_school;Uid=root;Pwd=12345;";

        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(connectionString);
        }

        public static void SetConnectionString(string server, string database, string user, string password)
        {
            connectionString = $"Server={server};Database={database};Uid={user};Pwd={password};";
        }

        public static bool TestConnection()
        {
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public static DataTable ExecuteQuery(string query, MySqlParameter[] parameters = null)
        {
            DataTable dt = new DataTable();
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        if (parameters != null)
                        {
                            cmd.Parameters.AddRange(parameters);
                        }
                        using (var adapter = new MySqlDataAdapter(cmd))
                        {
                            adapter.Fill(dt);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка виконання запиту: {ex.Message}", "Помилка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return dt;
        }

        public static int ExecuteNonQuery(string query, MySqlParameter[] parameters = null)
        {
            int result = 0;
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        if (parameters != null)
                        {
                            cmd.Parameters.AddRange(parameters);
                        }
                        result = cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка виконання команди: {ex.Message}", "Помилка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return result;
        }

        public static object ExecuteScalar(string query, MySqlParameter[] parameters = null)
        {
            object result = null;
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        if (parameters != null)
                        {
                            cmd.Parameters.AddRange(parameters);
                        }
                        result = cmd.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка виконання запиту: {ex.Message}", "Помилка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return result;
        }
    }
}