using System;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace MusicSchoolApp
{
    public partial class RegisterForm : Form
    {
        private TextBox txtUsername;
        private TextBox txtPassword;
        private TextBox txtConfirmPassword;
        private TextBox txtFullName;
        private ComboBox cmbRole;
        private Button btnRegister;
        private Button btnCancel;
        private Label lblTitle;

        public RegisterForm()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text = "Реєстрація нового користувача";
            this.Size = new Size(400, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            lblTitle = new Label
            {
                Text = "Реєстрація",
                Font = new Font("Arial", 16, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(140, 20)
            };

            Label lblUsername = new Label { Text = "Логін:", Location = new Point(50, 70), AutoSize = true };
            txtUsername = new TextBox { Location = new Point(50, 95), Size = new Size(300, 25) };

            Label lblPassword = new Label { Text = "Пароль:", Location = new Point(50, 130), AutoSize = true };
            txtPassword = new TextBox { Location = new Point(50, 155), Size = new Size(300, 25), PasswordChar = '●' };

            Label lblConfirmPassword = new Label { Text = "Підтвердження пароля:", Location = new Point(50, 190), AutoSize = true };
            txtConfirmPassword = new TextBox { Location = new Point(50, 215), Size = new Size(300, 25), PasswordChar = '●' };

            Label lblFullName = new Label { Text = "Повне ім'я:", Location = new Point(50, 250), AutoSize = true };
            txtFullName = new TextBox { Location = new Point(50, 275), Size = new Size(300, 25) };

            Label lblRole = new Label { Text = "Роль:", Location = new Point(50, 310), AutoSize = true };
            cmbRole = new ComboBox
            {
                Location = new Point(50, 335),
                Size = new Size(300, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbRole.Items.AddRange(new string[] { "staff", "admin" });
            cmbRole.SelectedIndex = 0;

            btnRegister = new Button
            {
                Text = "Зареєструватися",
                Location = new Point(50, 380),
                Size = new Size(140, 35),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnRegister.Click += BtnRegister_Click;

            btnCancel = new Button
            {
                Text = "Скасувати",
                Location = new Point(210, 380),
                Size = new Size(140, 35),
                BackColor = Color.FromArgb(244, 67, 54),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] {
                lblTitle, lblUsername, txtUsername, lblPassword, txtPassword,
                lblConfirmPassword, txtConfirmPassword, lblFullName, txtFullName,
                lblRole, cmbRole, btnRegister, btnCancel
            });
        }

        private void BtnRegister_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text) ||
                string.IsNullOrWhiteSpace(txtPassword.Text) ||
                string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                MessageBox.Show("Будь ласка, заповніть всі поля!", "Помилка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (txtPassword.Text != txtConfirmPassword.Text)
            {
                MessageBox.Show("Паролі не співпадають!", "Помилка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (txtPassword.Text.Length < 6)
            {
                MessageBox.Show("Пароль повинен містити мінімум 6 символів!", "Помилка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string checkQuery = "SELECT COUNT(*) FROM users WHERE username = @username";
            MySqlParameter[] checkParams = {
                new MySqlParameter("@username", txtUsername.Text.Trim())
            };

            object result = DatabaseHelper.ExecuteScalar(checkQuery, checkParams);
            if (Convert.ToInt32(result) > 0)
            {
                MessageBox.Show("Користувач з таким логіном вже існує!", "Помилка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string insertQuery = @"INSERT INTO users (username, password_hash, full_name, role) 
                                  VALUES (@username, @password_hash, @full_name, @role)";
            MySqlParameter[] insertParams = {
                new MySqlParameter("@username", txtUsername.Text.Trim()),
                new MySqlParameter("@password_hash", LoginForm.HashPassword(txtPassword.Text)),
                new MySqlParameter("@full_name", txtFullName.Text.Trim()),
                new MySqlParameter("@role", cmbRole.SelectedItem.ToString())
            };

            int rowsAffected = DatabaseHelper.ExecuteNonQuery(insertQuery, insertParams);

            if (rowsAffected > 0)
            {
                MessageBox.Show("Реєстрація успішна! Тепер ви можете увійти.", "Успіх",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            else
            {
                MessageBox.Show("Помилка при реєстрації. Спробуйте ще раз.", "Помилка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}