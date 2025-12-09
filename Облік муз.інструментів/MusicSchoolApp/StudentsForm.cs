using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace MusicSchoolApp
{
    public partial class StudentsForm : Form
    {
        private DataGridView dgvStudents;
        private TextBox txtSearch;
        private Button btnAdd, btnEdit, btnDelete, btnRefresh;
        private ComboBox cmbFilter, cmbStatusFilter;
        private Label lblInfo;

        public StudentsForm()
        {
            InitializeComponents();
            ApplyAccessControl();
            this.Load += StudentsForm_Load;
        }

        private void StudentsForm_Load(object sender, EventArgs e)
        {
            LoadStudents();
        }

        private void InitializeComponents()
        {
            this.Size = new Size(1100, 650);

            Label lblTitle = new Label
            {
                Text = "Управління учнями",
                Font = new Font("Arial", 16, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };

            lblInfo = new Label
            {
                Text = "ℹ️ Staff: Видалення недоступне",
                ForeColor = Color.DarkBlue,
                Location = new Point(20, 50),
                Size = new Size(1050, 20),
                Visible = !UserSession.IsAdmin
            };

            Label lblSearch = new Label
            {
                Text = "Пошук:",
                Location = new Point(20, 85),
                AutoSize = true
            };

            txtSearch = new TextBox
            {
                Location = new Point(80, 83),
                Size = new Size(200, 25)
            };
            txtSearch.TextChanged += (s, e) => LoadStudents();

            Label lblFilter = new Label
            {
                Text = "Клас:",
                Location = new Point(300, 85),
                AutoSize = true
            };

            cmbFilter = new ComboBox
            {
                Location = new Point(350, 83),
                Size = new Size(120, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbFilter.Items.AddRange(new string[] { "Всі", "1 клас", "2 клас", "3 клас", "4 клас", "5 клас", "6 клас", "7 клас" });
            cmbFilter.SelectedIndex = 0;
            cmbFilter.SelectedIndexChanged += (s, e) => LoadStudents();

            btnAdd = new Button
            {
                Text = "Додати",
                Location = new Point(20, 120),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White
            };
            btnAdd.Click += BtnAdd_Click;

            btnEdit = new Button
            {
                Text = "Редагувати",
                Location = new Point(130, 120),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(33, 150, 243),
                ForeColor = Color.White
            };
            btnEdit.Click += BtnEdit_Click;

            btnDelete = new Button
            {
                Text = "Видалити",
                Location = new Point(240, 120),
                Size = new Size(120, 30),
                BackColor = Color.FromArgb(244, 67, 54),
                ForeColor = Color.White
            };
            btnDelete.Click += BtnDelete_Click;

            btnRefresh = new Button
            {
                Text = "Оновити",
                Location = new Point(370, 120),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(158, 158, 158),
                ForeColor = Color.White
            };
            btnRefresh.Click += (s, e) => LoadStudents();

            dgvStudents = new DataGridView
            {
                Location = new Point(20, 160),
                Size = new Size(1050, 450),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            this.Controls.AddRange(new Control[] {
                lblTitle, lblInfo, lblSearch, txtSearch, lblFilter, cmbFilter,
                btnAdd, btnEdit, btnDelete, btnRefresh, dgvStudents
            });
        }

        private void ApplyAccessControl()
        {
            if (!UserSession.CanDeleteStudents)
            {
                lblInfo.Visible = true;
            }
        }

        private void LoadStudents()
        {
            string query = @"SELECT student_id AS 'ID', 
                           CONCAT(last_name, ' ', first_name) AS 'ПІБ',
                           birth_date AS 'Дата народження',
                           class AS 'Клас',
                           enrollment_year AS 'Рік вступу',
                           phone AS 'Телефон',
                           email AS 'Email'
                    FROM students
                    WHERE 1=1";

            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                query += $" AND (first_name LIKE '%{txtSearch.Text}%' OR last_name LIKE '%{txtSearch.Text}%')";
            }

            if (cmbFilter.SelectedIndex > 0)
            {
                query += $" AND class = '{cmbFilter.SelectedItem}'";
            }

            query += " ORDER BY last_name, first_name";

            dgvStudents.DataSource = DatabaseHelper.ExecuteQuery(query);

            if (dgvStudents.Columns.Contains("ID"))
            {
                dgvStudents.Columns["ID"].Width = 50;
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (!UserSession.CanAddStudents)
            {
                MessageBox.Show("У вас немає прав для додавання учнів!",
                    "Доступ заборонено", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            StudentEditForm form = new StudentEditForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadStudents();
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (!UserSession.CanEditStudents)
            {
                MessageBox.Show("У вас немає прав для редагування учнів!",
                    "Доступ заборонено", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (dgvStudents.SelectedRows.Count == 0)
            {
                MessageBox.Show("Оберіть учня для редагування!", "Увага",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int studentId = Convert.ToInt32(dgvStudents.SelectedRows[0].Cells["ID"].Value);
            StudentEditForm form = new StudentEditForm(studentId);
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadStudents();
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvStudents.SelectedRows.Count == 0)
            {
                MessageBox.Show("Оберіть учня!", "Увага",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!UserSession.CanDeleteStudents)
            {
                MessageBox.Show("У вас немає прав для видалення учнів!",
                    "Доступ заборонено", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int studentId = Convert.ToInt32(dgvStudents.SelectedRows[0].Cells["ID"].Value);
            string studentName = dgvStudents.SelectedRows[0].Cells["ПІБ"].Value.ToString();

            var result = MessageBox.Show(
                $"Видалити учня '{studentName}'?\n" +
                "Цю дію неможливо скасувати.",
                "Підтвердження видалення",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                string query = "DELETE FROM students WHERE student_id = @id";
                MySqlParameter[] parameters = { new MySqlParameter("@id", studentId) };

                if (DatabaseHelper.ExecuteNonQuery(query, parameters) > 0)
                {
                    MessageBox.Show("Учня видалено!", "Успіх",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadStudents();
                }
            }
        }
    }
}