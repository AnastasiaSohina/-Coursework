using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace MusicSchoolApp
{
    public partial class UsersForm : Form
    {
        private DataGridView dgv;
        private Button btnDelete, btnRefresh;

        public UsersForm()
        {
            InitializeComponents();
            LoadData();
        }

        private void InitializeComponents()
        {
            this.Size = new Size(800, 500);

            Label lblTitle = new Label
            {
                Text = "Управління користувачами",
                Font = new Font("Arial", 16, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };

            btnDelete = new Button
            {
                Text = "Видалити",
                Location = new Point(20, 60),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(244, 67, 54),
                ForeColor = Color.White
            };
            btnDelete.Click += BtnDelete_Click;

            btnRefresh = new Button
            {
                Text = "Оновити",
                Location = new Point(130, 60),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(158, 158, 158),
                ForeColor = Color.White
            };
            btnRefresh.Click += (s, e) => LoadData();

            dgv = new DataGridView
            {
                Location = new Point(20, 100),
                Size = new Size(750, 350),
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            this.Controls.AddRange(new Control[] { lblTitle, btnDelete, btnRefresh, dgv });
        }

        private void LoadData()
        {
            dgv.DataSource = DatabaseHelper.ExecuteQuery("SELECT user_id AS 'ID', username AS 'Логін', full_name AS 'ПІБ', role AS 'Роль', created_at AS 'Створено' FROM users ORDER BY created_at DESC");
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0) { MessageBox.Show("Оберіть користувача!"); return; }

            int userId = Convert.ToInt32(dgv.SelectedRows[0].Cells["ID"].Value);
            if (userId == UserSession.UserId)
            {
                MessageBox.Show("Не можна видалити себе!", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Видалити користувача?", "Підтвердження", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                DatabaseHelper.ExecuteNonQuery("DELETE FROM users WHERE user_id = @id", new[] { new MySqlParameter("@id", userId) });
                LoadData();
            }
        }
    }
}