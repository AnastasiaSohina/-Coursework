using System;
using System.Windows.Forms;

namespace MusicSchoolApp
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (!DatabaseHelper.TestConnection())
            {
                var result = MessageBox.Show(
                    "Не вдалося підключитися до бази даних!\n\n" +
                    "Переконайтеся, що:\n" +
                    "1. MySQL сервер запущено\n" +
                    "2. База даних 'music_school' створена\n" +
                    "3. Параметри підключення вірні\n\n" +
                    "Відкрити налаштування підключення?",
                    "Помилка підключення",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Error
                );

                if (result == DialogResult.Yes)
                {
                    DatabaseConfigForm configForm = new DatabaseConfigForm();
                    if (configForm.ShowDialog() != DialogResult.OK)
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
            }

            Application.Run(new LoginForm());
        }
    }

    public partial class DatabaseConfigForm : Form
    {
        private TextBox txtServer, txtDatabase, txtUser, txtPassword;
        private Button btnSave, btnCancel, btnTest;

        public DatabaseConfigForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Налаштування підключення до бази даних";
            this.Size = new System.Drawing.Size(400, 280);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            int y = 20;

            Label lblServer = new Label { Text = "Сервер:", Location = new System.Drawing.Point(20, y), AutoSize = true };
            txtServer = new TextBox { Location = new System.Drawing.Point(120, y), Size = new System.Drawing.Size(240, 25), Text = "localhost" };
            this.Controls.AddRange(new Control[] { lblServer, txtServer });
            y += 40;

            Label lblDatabase = new Label { Text = "База даних:", Location = new System.Drawing.Point(20, y), AutoSize = true };
            txtDatabase = new TextBox { Location = new System.Drawing.Point(120, y), Size = new System.Drawing.Size(240, 25), Text = "music_school" };
            this.Controls.AddRange(new Control[] { lblDatabase, txtDatabase });
            y += 40;

            Label lblUser = new Label { Text = "Користувач:", Location = new System.Drawing.Point(20, y), AutoSize = true };
            txtUser = new TextBox { Location = new System.Drawing.Point(120, y), Size = new System.Drawing.Size(240, 25), Text = "root" };
            this.Controls.AddRange(new Control[] { lblUser, txtUser });
            y += 40;

            Label lblPassword = new Label { Text = "Пароль:", Location = new System.Drawing.Point(20, y), AutoSize = true };
            txtPassword = new TextBox { Location = new System.Drawing.Point(120, y), Size = new System.Drawing.Size(240, 25), PasswordChar = '●' };
            this.Controls.AddRange(new Control[] { lblPassword, txtPassword });
            y += 50;

            btnTest = new Button
            {
                Text = "Тест підключення",
                Location = new System.Drawing.Point(20, y),
                Size = new System.Drawing.Size(130, 35),
                BackColor = System.Drawing.Color.FromArgb(33, 150, 243),
                ForeColor = System.Drawing.Color.White
            };
            btnTest.Click += BtnTest_Click;

            btnSave = new Button
            {
                Text = "Зберегти",
                Location = new System.Drawing.Point(160, y),
                Size = new System.Drawing.Size(100, 35),
                BackColor = System.Drawing.Color.FromArgb(76, 175, 80),
                ForeColor = System.Drawing.Color.White,
                DialogResult = DialogResult.OK
            };
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button
            {
                Text = "Скасувати",
                Location = new System.Drawing.Point(270, y),
                Size = new System.Drawing.Size(100, 35),
                DialogResult = DialogResult.Cancel
            };

            this.Controls.AddRange(new Control[] { btnTest, btnSave, btnCancel });
        }

        private void BtnTest_Click(object sender, EventArgs e)
        {
            DatabaseHelper.SetConnectionString(txtServer.Text, txtDatabase.Text, txtUser.Text, txtPassword.Text);

            if (DatabaseHelper.TestConnection())
            {
                MessageBox.Show("Підключення успішне!", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Не вдалося підключитися. Перевірте параметри!", "Помилка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            DatabaseHelper.SetConnectionString(txtServer.Text, txtDatabase.Text, txtUser.Text, txtPassword.Text);

            if (!DatabaseHelper.TestConnection())
            {
                MessageBox.Show("Підключення неможливе. Збережіть інші параметри!", "Попередження",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.None;
            }
        }
    }
}