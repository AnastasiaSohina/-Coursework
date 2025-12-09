using System;
using System.Data;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace MusicSchoolApp
{
    public partial class LoginForm : Form
    {
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;
        private Button btnRegister;
        private Label lblTitle;
        private Label lblUsername;
        private Label lblPassword;

        public LoginForm()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text = "Вхід до системи - Музична школа";
            this.Size = new Size(400, 300);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            lblTitle = new Label
            {
                Text = "Музична школа",
                Font = new Font("Arial", 16, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(120, 20)
            };

            lblUsername = new Label
            {
                Text = "Логін:",
                Location = new Point(50, 80),
                AutoSize = true
            };

            txtUsername = new TextBox
            {
                Location = new Point(50, 105),
                Size = new Size(300, 25)
            };

            lblPassword = new Label
            {
                Text = "Пароль:",
                Location = new Point(50, 140),
                AutoSize = true
            };

            txtPassword = new TextBox
            {
                Location = new Point(50, 165),
                Size = new Size(300, 25),
                PasswordChar = '●'
            };

            btnLogin = new Button
            {
                Text = "Увійти",
                Location = new Point(50, 210),
                Size = new Size(140, 35),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnLogin.Click += BtnLogin_Click;

            btnRegister = new Button
            {
                Text = "Реєстрація",
                Location = new Point(210, 210),
                Size = new Size(140, 35),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnRegister.Click += BtnRegister_Click;

            this.Controls.AddRange(new Control[] {
                lblTitle, lblUsername, txtUsername, lblPassword, txtPassword, btnLogin, btnRegister
            });

            this.AcceptButton = btnLogin;
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Будь ласка, заповніть всі поля!", "Помилка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = "SELECT user_id, username, full_name, role, password_hash FROM users WHERE username = @username";
            MySqlParameter[] parameters = {
                new MySqlParameter("@username", txtUsername.Text.Trim())
            };

            DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters);

            if (dt.Rows.Count > 0)
            {
                string storedHash = dt.Rows[0]["password_hash"].ToString();
                string inputHash = HashPassword(txtPassword.Text);

                if (storedHash == inputHash)
                {
                    UserSession.Login(
                        Convert.ToInt32(dt.Rows[0]["user_id"]),
                        dt.Rows[0]["username"].ToString(),
                        dt.Rows[0]["full_name"].ToString(),
                        dt.Rows[0]["role"].ToString()
                    );

                    MessageBox.Show($"Вітаємо, {UserSession.FullName}!", "Успіх",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    MainForm mainForm = new MainForm();
                    this.Hide();
                    mainForm.ShowDialog();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Невірний пароль!", "Помилка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Користувача не знайдено!", "Помилка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnRegister_Click(object sender, EventArgs e)
        {
            RegisterForm registerForm = new RegisterForm();
            registerForm.ShowDialog();
        }

        public static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}