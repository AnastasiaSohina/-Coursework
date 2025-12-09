using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace MusicSchoolApp
{
    public partial class StudentSubjectsForm : Form
    {
        private DataGridView dgv;
        private Button btnAdd, btnEdit, btnDelete;
        private ComboBox cmbStudent;
        private Label lblAccessInfo;
        public StudentSubjectsForm()
        {
            InitializeComponents();
            LoadComboBox();
            ApplyPermissions();
            LoadData();
        }
        private void ApplyPermissions()
        {
            if (!UserSession.IsAdmin)
            {
                lblAccessInfo.Visible = true;

                btnAdd.Enabled = false;
                btnAdd.BackColor = Color.Gray;
                btnAdd.Cursor = Cursors.No;

                btnDelete.Enabled = false;
                btnDelete.BackColor = Color.Gray;
                btnDelete.Cursor = Cursors.No;
            }
        }
        private void InitializeComponents()
        {
            this.Size = new Size(1000, 600);

            Label lblTitle = new Label
            {
                Text = "Запис учнів на предмети",
                Font = new Font("Arial", 16, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };

            lblAccessInfo = new Label
            {
                Text = "ℹ️ Staff: Ви можете редагувати. Створення/видалення - тільки для адміністраторів.",
                ForeColor = Color.DarkBlue,
                Location = new Point(20, 50),
                Size = new Size(950, 20),
                Visible = false
            };

            Label lblStudent = new Label
            {
                Text = "Фільтр по учню:",
                Location = new Point(20, 85),
                AutoSize = true
            };
            cmbStudent = new ComboBox
            {
                Location = new Point(140, 82),
                Size = new Size(250, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbStudent.SelectedIndexChanged += (s, e) => LoadData();

            btnAdd = new Button
            {
                Text = "Записати",
                Location = new Point(20, 120),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White
            };
            btnAdd.Click += (s, e) =>
            {
                StudentSubjectEditForm f = new StudentSubjectEditForm();
                if (f.ShowDialog() == DialogResult.OK) LoadData();
            };

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
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(244, 67, 54),
                ForeColor = Color.White
            };
            btnDelete.Click += BtnDelete_Click;

            dgv = new DataGridView
            {
                Location = new Point(20, 160),
                Size = new Size(950, 380),
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            this.Controls.AddRange(new Control[]
            {
                lblTitle, lblAccessInfo,
                lblStudent, cmbStudent,
                btnAdd, btnEdit, btnDelete,
                dgv
            });
        }
        private void LoadComboBox()
        {
            DataTable dt = DatabaseHelper.ExecuteQuery("SELECT 0 AS student_id, 'Всі учні' AS name UNION SELECT student_id, CONCAT(last_name, ' ', first_name) AS name FROM students ORDER BY name");
            cmbStudent.DisplayMember = "name";
            cmbStudent.ValueMember = "student_id";
            cmbStudent.DataSource = dt;
        }

        private void LoadData()
        {
            string query = @"SELECT ss.id AS 'ID',
                CONCAT(s.last_name, ' ', s.first_name) AS 'Учень',
                subj.name AS 'Предмет',
                CONCAT(t.last_name, ' ', t.first_name) AS 'Викладач',
                ss.start_date AS 'Дата початку',
                ss.end_date AS 'Дата закінчення'
            FROM student_subjects ss
            JOIN students s ON ss.student_id = s.student_id
            JOIN subjects subj ON ss.subject_id = subj.subject_id
            LEFT JOIN teachers t ON ss.teacher_id = t.teacher_id
            WHERE 1=1";

            if (cmbStudent.SelectedValue != null && Convert.ToInt32(cmbStudent.SelectedValue) > 0)
                query += $" AND ss.student_id = {cmbStudent.SelectedValue}";

            query += " ORDER BY s.last_name, subj.name";
            dgv.DataSource = DatabaseHelper.ExecuteQuery(query);
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0) { MessageBox.Show("Оберіть запис!"); return; }
            StudentSubjectEditForm f = new StudentSubjectEditForm(Convert.ToInt32(dgv.SelectedRows[0].Cells["ID"].Value));
            if (f.ShowDialog() == DialogResult.OK) LoadData();
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0) { MessageBox.Show("Оберіть запис!"); return; }
            if (MessageBox.Show("Видалити запис?", "Підтвердження", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                DatabaseHelper.ExecuteNonQuery("DELETE FROM student_subjects WHERE id = @id",
                    new[] { new MySqlParameter("@id", dgv.SelectedRows[0].Cells["ID"].Value) });
                LoadData();
            }
        }
    }

    public partial class StudentSubjectEditForm : Form
    {
        private int? id;
        private ComboBox cmbStudent, cmbSubject, cmbTeacher;
        private DateTimePicker dtpStart, dtpEnd;
        private CheckBox chkHasEnd;

        public StudentSubjectEditForm(int? recordId = null)
        {
            id = recordId;
            InitializeComponent();
            LoadComboBoxes();
            if (id.HasValue) LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = id.HasValue ? "Редагування запису" : "Запис учня на предмет";
            this.Size = new Size(450, 380);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            int y = 20;
            Label lblStudent = new Label { Text = "Учень:", Location = new Point(20, y), AutoSize = true };
            cmbStudent = new ComboBox { Location = new Point(150, y), Size = new Size(270, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            this.Controls.AddRange(new Control[] { lblStudent, cmbStudent });
            y += 40;

            Label lblSubject = new Label { Text = "Предмет:", Location = new Point(20, y), AutoSize = true };
            cmbSubject = new ComboBox { Location = new Point(150, y), Size = new Size(270, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            this.Controls.AddRange(new Control[] { lblSubject, cmbSubject });
            y += 40;

            Label lblTeacher = new Label { Text = "Викладач:", Location = new Point(20, y), AutoSize = true };
            cmbTeacher = new ComboBox { Location = new Point(150, y), Size = new Size(270, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            this.Controls.AddRange(new Control[] { lblTeacher, cmbTeacher });
            y += 40;

            Label lblStart = new Label { Text = "Дата початку:", Location = new Point(20, y), AutoSize = true };
            dtpStart = new DateTimePicker { Location = new Point(150, y), Size = new Size(270, 25), Format = DateTimePickerFormat.Short };
            this.Controls.AddRange(new Control[] { lblStart, dtpStart });
            y += 40;

            chkHasEnd = new CheckBox { Text = "Вказати дату закінчення", Location = new Point(20, y), AutoSize = true };
            chkHasEnd.CheckedChanged += (s, e) => dtpEnd.Enabled = chkHasEnd.Checked;
            this.Controls.Add(chkHasEnd);
            y += 30;

            Label lblEnd = new Label { Text = "Дата закінчення:", Location = new Point(20, y), AutoSize = true };
            dtpEnd = new DateTimePicker { Location = new Point(150, y), Size = new Size(270, 25), Format = DateTimePickerFormat.Short, Enabled = false };
            this.Controls.AddRange(new Control[] { lblEnd, dtpEnd });
            y += 50;

            Button btnSave = new Button
            {
                Text = "Зберегти",
                Location = new Point(150, y),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                DialogResult = DialogResult.OK
            };
            btnSave.Click += BtnSave_Click;

            Button btnCancel = new Button { Text = "Скасувати", Location = new Point(260, y), Size = new Size(100, 35), DialogResult = DialogResult.Cancel };
            this.Controls.AddRange(new Control[] { btnSave, btnCancel });
        }

        private void LoadComboBoxes()
        {
            cmbStudent.DataSource = DatabaseHelper.ExecuteQuery("SELECT student_id, CONCAT(last_name, ' ', first_name) AS name FROM students ORDER BY last_name");
            cmbStudent.DisplayMember = "name";
            cmbStudent.ValueMember = "student_id";

            cmbSubject.DataSource = DatabaseHelper.ExecuteQuery("SELECT subject_id, name FROM subjects ORDER BY name");
            cmbSubject.DisplayMember = "name";
            cmbSubject.ValueMember = "subject_id";

            cmbTeacher.DataSource = DatabaseHelper.ExecuteQuery("SELECT teacher_id, CONCAT(last_name, ' ', first_name) AS name FROM teachers ORDER BY last_name");
            cmbTeacher.DisplayMember = "name";
            cmbTeacher.ValueMember = "teacher_id";
        }

        private void LoadData()
        {
            DataTable dt = DatabaseHelper.ExecuteQuery("SELECT * FROM student_subjects WHERE id = @id", new[] { new MySqlParameter("@id", id) });
            if (dt.Rows.Count > 0)
            {
                cmbStudent.SelectedValue = dt.Rows[0]["student_id"];
                cmbSubject.SelectedValue = dt.Rows[0]["subject_id"];
                cmbTeacher.SelectedValue = dt.Rows[0]["teacher_id"];
                dtpStart.Value = Convert.ToDateTime(dt.Rows[0]["start_date"]);
                if (dt.Rows[0]["end_date"] != DBNull.Value)
                {
                    chkHasEnd.Checked = true;
                    dtpEnd.Value = Convert.ToDateTime(dt.Rows[0]["end_date"]);
                }
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            string query = id.HasValue
                ? "UPDATE student_subjects SET student_id=@sid, subject_id=@subid, teacher_id=@tid, start_date=@sdate, end_date=@edate WHERE id=@id"
                : "INSERT INTO student_subjects (student_id, subject_id, teacher_id, start_date, end_date) VALUES (@sid, @subid, @tid, @sdate, @edate)";

            var p = new[] {
                new MySqlParameter("@sid", cmbStudent.SelectedValue),
                new MySqlParameter("@subid", cmbSubject.SelectedValue),
                new MySqlParameter("@tid", cmbTeacher.SelectedValue),
                new MySqlParameter("@sdate", dtpStart.Value.Date),
                new MySqlParameter("@edate", chkHasEnd.Checked ? (object)dtpEnd.Value.Date : DBNull.Value)
            };

            if (id.HasValue)
            {
                Array.Resize(ref p, 6);
                p[5] = new MySqlParameter("@id", id);
            }

            DatabaseHelper.ExecuteNonQuery(query, p);
            MessageBox.Show("Збережено!");
        }
    }
}