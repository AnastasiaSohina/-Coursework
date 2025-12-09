using System;
using System.Drawing;
using System.Windows.Forms;

namespace MusicSchoolApp
{
    public partial class MainForm : Form
    {
        private MenuStrip menuStrip;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel statusLabel;
        private Panel mainPanel;

        public MainForm()
        {
            InitializeComponents();
            UpdateStatusBar();
        }

        private void InitializeComponents()
        {
            this.Text = "Система управління музичною школою";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;

            menuStrip = new MenuStrip();

            ToolStripMenuItem studentsMenu = new ToolStripMenuItem("Учні");
            studentsMenu.DropDownItems.Add("Список учнів", null, (s, e) => OpenForm(new StudentsForm()));
            studentsMenu.DropDownItems.Add("Запис на предмети", null, (s, e) => OpenForm(new StudentSubjectsForm()));

            ToolStripMenuItem teachersMenu = new ToolStripMenuItem("Викладачі");
            teachersMenu.DropDownItems.Add("Список викладачів", null, (s, e) => OpenForm(new TeachersForm()));

            ToolStripMenuItem instrumentsMenu = new ToolStripMenuItem("Інструменти");
            instrumentsMenu.DropDownItems.Add("Список інструментів", null, (s, e) => OpenForm(new InstrumentsForm()));
            instrumentsMenu.DropDownItems.Add("Видача інструментів", null, (s, e) => OpenForm(new InstrumentAssignmentsForm()));
            instrumentsMenu.DropDownItems.Add("Ремонт інструментів", null, (s, e) => OpenForm(new InstrumentRepairsForm()));
            instrumentsMenu.DropDownItems.Add("Постачальники", null, (s, e) => OpenForm(new SuppliersForm()));

            ToolStripMenuItem educationMenu = new ToolStripMenuItem("Навчання");
            educationMenu.DropDownItems.Add("Предмети", null, (s, e) => OpenForm(new SubjectsForm()));
            educationMenu.DropDownItems.Add("Розклад занять", null, (s, e) => OpenForm(new ScheduleForm()));
            educationMenu.DropDownItems.Add("Аудиторії", null, (s, e) => OpenForm(new RoomsForm()));

            ToolStripMenuItem reportsMenu = new ToolStripMenuItem("Звіти");
            reportsMenu.DropDownItems.Add("Звіт по учням", null, (s, e) => OpenForm(new ReportsForm()));

            ToolStripMenuItem settingsMenu = new ToolStripMenuItem("Налаштування");

            if (UserSession.CanManageUsers)
            {
                settingsMenu.DropDownItems.Add("Користувачі", null, (s, e) => OpenForm(new UsersForm()));
                settingsMenu.DropDownItems.Add(new ToolStripSeparator());
            }

            settingsMenu.DropDownItems.Add("Вихід", null, (s, e) => Logout());

            menuStrip.Items.AddRange(new ToolStripItem[] {
                studentsMenu, teachersMenu, instrumentsMenu, educationMenu, reportsMenu, settingsMenu
            });

            statusStrip = new StatusStrip();
            statusLabel = new ToolStripStatusLabel();
            statusStrip.Items.Add(statusLabel);

            mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(240, 240, 240)
            };

            Label welcomeLabel = new Label
            {
                Text = $"Вітаємо, {UserSession.FullName}!\n\nВаша роль: {UserSession.Role}\n\nОберіть розділ з меню для роботи.",
                Font = new Font("Arial", 14),
                AutoSize = true,
                Location = new Point(50, 50)
            };
            mainPanel.Controls.Add(welcomeLabel);

            this.Controls.Add(mainPanel);
            this.Controls.Add(statusStrip);
            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);
        }

        private void OpenForm(Form form)
        {
            mainPanel.Controls.Clear();
            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;
            mainPanel.Controls.Add(form);
            form.Show();
        }

        private void UpdateStatusBar()
        {
            statusLabel.Text = $"Користувач: {UserSession.Username} ({UserSession.Role}) | " +
                             $"Час входу: {UserSession.LoginTime:HH:mm:ss}";
        }

        private void Logout()
        {
            var result = MessageBox.Show("Ви дійсно бажаєте вийти?", "Підтвердження",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                UserSession.Logout();
                this.Close();
                LoginForm loginForm = new LoginForm();
                loginForm.Show();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (UserSession.IsLoggedIn)
            {
                var result = MessageBox.Show("Закрити програму?", "Підтвердження",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                }
                else
                {
                    Application.Exit();
                }
            }
            base.OnFormClosing(e);
        }
    }
}