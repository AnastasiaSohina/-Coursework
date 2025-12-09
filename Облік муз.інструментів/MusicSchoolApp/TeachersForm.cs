using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace MusicSchoolApp
{
    public partial class TeachersForm : Form
    {
        private DataGridView dgvTeachers;
        private TextBox txtSearch;
        private Button btnAdd, btnEdit, btnDelete, btnRefresh;
        private Label lblAccessInfo;

        public TeachersForm()
        {
            InitializeComponents();
            ApplyAccessControl(); 
            LoadTeachers();
        }

        private void ApplyAccessControl()
        {
            if (!UserSession.IsAdmin)
            {
               
                lblAccessInfo.Visible = true;

               
                btnAdd.Enabled = false;
                btnAdd.BackColor = Color.Gray;
                btnAdd.Cursor = Cursors.No;

                btnEdit.Enabled = false;
                btnEdit.BackColor = Color.Gray;
                btnEdit.Cursor = Cursors.No;

                btnDelete.Enabled = false;
                btnDelete.BackColor = Color.Gray;
                btnDelete.Cursor = Cursors.No;
            }
        }

        private void InitializeComponents()
        {
            this.Size = new Size(900, 600);

            Label lblTitle = new Label
            {
                Text = "Управління викладачами",
                Font = new Font("Arial", 16, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };

            lblAccessInfo = new Label
            {
                Text = "⚠️ Ви маєте права тільки для перегляду. Редагування доступне лише адміністраторам.",
                ForeColor = Color.DarkOrange,
                Location = new Point(20, 50),
                Size = new Size(850, 20),
                Visible = false 
            };

            Label lblSearch = new Label { Text = "Пошук:", Location = new Point(20, 85), AutoSize = true };
            txtSearch = new TextBox { Location = new Point(80, 83), Size = new Size(250, 25) };
            txtSearch.TextChanged += (s, e) => LoadTeachers();

            btnAdd = CreateButton("Додати", new Point(20, 120), Color.FromArgb(76, 175, 80));
            btnAdd.Click += BtnAdd_Click;

            btnEdit = CreateButton("Редагувати", new Point(130, 120), Color.FromArgb(33, 150, 243));
            btnEdit.Click += BtnEdit_Click;

            btnDelete = CreateButton("Видалити", new Point(240, 120), Color.FromArgb(244, 67, 54));
            btnDelete.Click += BtnDelete_Click;

            btnRefresh = CreateButton("Оновити", new Point(350, 120), Color.FromArgb(158, 158, 158));
            btnRefresh.Click += (s, e) => LoadTeachers();

            dgvTeachers = new DataGridView
            {
                Location = new Point(20, 160),
                Size = new Size(850, 380),
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            this.Controls.AddRange(new Control[] {
                lblTitle, lblAccessInfo, lblSearch, txtSearch, btnAdd, btnEdit, btnDelete, btnRefresh, dgvTeachers
            });
        }

        private Button CreateButton(string text, Point location, Color backColor)
        {
            return new Button
            {
                Text = text,
                Location = location,
                Size = new Size(100, 30),
                BackColor = backColor,
                ForeColor = Color.White
            };
        }

        private void LoadTeachers()
        {
            string query = @"SELECT teacher_id AS 'ID',
                                   CONCAT(last_name, ' ', first_name) AS 'ПІБ',
                                   specialization AS 'Спеціалізація',
                                   phone AS 'Телефон',
                                   email AS 'Email'
                            FROM teachers WHERE 1=1";

            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                query += $" AND (first_name LIKE '%{txtSearch.Text}%' OR last_name LIKE '%{txtSearch.Text}%' OR specialization LIKE '%{txtSearch.Text}%')";
            }

            query += " ORDER BY last_name, first_name";
            dgvTeachers.DataSource = DatabaseHelper.ExecuteQuery(query);
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (!UserSession.IsAdmin)
            {
                MessageBox.Show("У вас немає прав для додавання викладачів!\n\nЦя функція доступна тільки адміністраторам.",
                    "Доступ заборонено", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            TeacherEditForm form = new TeacherEditForm();
            if (form.ShowDialog() == DialogResult.OK) LoadTeachers();
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (!UserSession.IsAdmin)
            {
                MessageBox.Show("У вас немає прав для редагування викладачів!\n\nЦя функція доступна тільки адміністраторам.",
                    "Доступ заборонено", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (dgvTeachers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Оберіть викладача!", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            int id = Convert.ToInt32(dgvTeachers.SelectedRows[0].Cells["ID"].Value);
            TeacherEditForm form = new TeacherEditForm(id);
            if (form.ShowDialog() == DialogResult.OK) LoadTeachers();
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (!UserSession.IsAdmin)
            {
                MessageBox.Show("У вас немає прав для видалення викладачів!\n\nЦя функція доступна тільки адміністраторам.",
                    "Доступ заборонено", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (dgvTeachers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Оберіть викладача!", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Видалити викладача?", "Підтвердження",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                int id = Convert.ToInt32(dgvTeachers.SelectedRows[0].Cells["ID"].Value);
                string query = "DELETE FROM teachers WHERE teacher_id = @id";
                if (DatabaseHelper.ExecuteNonQuery(query, new[] { new MySqlParameter("@id", id) }) > 0)
                {
                    MessageBox.Show("Викладача видалено!", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadTeachers();
                }
            }
        }
    }

    public partial class TeacherEditForm : Form
    {
        private int? teacherId;
        private TextBox txtFirstName, txtLastName, txtSpecialization, txtPhone, txtEmail, txtNotes;
        private Button btnSave, btnCancel;

        public TeacherEditForm(int? id = null)
        {
            teacherId = id;
            InitializeComponent();
            if (teacherId.HasValue) LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = teacherId.HasValue ? "Редагування викладача" : "Додавання викладача";
            this.Size = new Size(450, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            int y = 20;
            AddField("Прізвище:", ref txtLastName, ref y);
            AddField("Ім'я:", ref txtFirstName, ref y);
            AddField("Спеціалізація:", ref txtSpecialization, ref y);
            AddField("Телефон:", ref txtPhone, ref y);
            AddField("Email:", ref txtEmail, ref y);

            Label lblNotes = new Label { Text = "Примітки:", Location = new Point(20, y), AutoSize = true };
            txtNotes = new TextBox { Location = new Point(150, y), Size = new Size(250, 80), Multiline = true };
            this.Controls.AddRange(new Control[] { lblNotes, txtNotes });
            y += 95;

            btnSave = new Button
            {
                Text = "Зберегти",
                Location = new Point(150, y),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                DialogResult = DialogResult.OK
            };
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button
            {
                Text = "Скасувати",
                Location = new Point(260, y),
                Size = new Size(100, 35),
                DialogResult = DialogResult.Cancel
            };

            this.Controls.AddRange(new Control[] { btnSave, btnCancel });
        }

        private void AddField(string label, ref TextBox textBox, ref int y)
        {
            Label lbl = new Label { Text = label, Location = new Point(20, y), AutoSize = true };
            textBox = new TextBox { Location = new Point(150, y), Size = new Size(250, 25) };
            this.Controls.AddRange(new Control[] { lbl, textBox });
            y += 40;
        }

        private void LoadData()
        {
            DataTable dt = DatabaseHelper.ExecuteQuery("SELECT * FROM teachers WHERE teacher_id = @id",
                new[] { new MySqlParameter("@id", teacherId) });
            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                txtFirstName.Text = row["first_name"].ToString();
                txtLastName.Text = row["last_name"].ToString();
                txtSpecialization.Text = row["specialization"].ToString();
                txtPhone.Text = row["phone"].ToString();
                txtEmail.Text = row["email"].ToString();
                txtNotes.Text = row["notes"].ToString();
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFirstName.Text) || string.IsNullOrWhiteSpace(txtLastName.Text))
            {
                MessageBox.Show("Заповніть обов'язкові поля!", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.None;
                return;
            }

            string query = teacherId.HasValue
                ? "UPDATE teachers SET first_name=@fn, last_name=@ln, specialization=@spec, phone=@phone, email=@email, notes=@notes WHERE teacher_id=@id"
                : "INSERT INTO teachers (first_name, last_name, specialization, phone, email, notes) VALUES (@fn, @ln, @spec, @phone, @email, @notes)";

            var parameters = new[]
            {
                new MySqlParameter("@fn", txtFirstName.Text),
                new MySqlParameter("@ln", txtLastName.Text),
                new MySqlParameter("@spec", txtSpecialization.Text),
                new MySqlParameter("@phone", txtPhone.Text),
                new MySqlParameter("@email", txtEmail.Text),
                new MySqlParameter("@notes", txtNotes.Text)
            };

            if (teacherId.HasValue)
            {
                Array.Resize(ref parameters, parameters.Length + 1);
                parameters[parameters.Length - 1] = new MySqlParameter("@id", teacherId);
            }

            if (DatabaseHelper.ExecuteNonQuery(query, parameters) > 0)
                MessageBox.Show("Збережено!", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                this.DialogResult = DialogResult.None;
        }
    }
}