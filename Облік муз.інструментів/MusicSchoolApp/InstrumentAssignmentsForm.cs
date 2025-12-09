using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace MusicSchoolApp
{
    public partial class InstrumentAssignmentsForm : Form
    {
        private DataGridView dgv;
        private Button btnAdd, btnEdit, btnReturn, btnDelete;
        private CheckBox chkShowReturned;

        public InstrumentAssignmentsForm()
        {
            InitializeComponents();
            LoadData();
        }

        private void InitializeComponents()
        {
            this.Size = new Size(1200, 600);

            Label lblTitle = new Label
            {
                Text = "Видача інструментів учням",
                Font = new Font("Arial", 16, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };

            chkShowReturned = new CheckBox
            {
                Text = "Показати повернені",
                Location = new Point(20, 60),
                AutoSize = true
            };
            chkShowReturned.CheckedChanged += (s, e) => LoadData();

            btnAdd = new Button
            {
                Text = "Видати інструмент",
                Location = new Point(20, 90),
                Size = new Size(140, 30),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White
            };
            btnAdd.Click += BtnAdd_Click;

            btnReturn = new Button
            {
                Text = "Повернути",
                Location = new Point(170, 90),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(255, 152, 0),
                ForeColor = Color.White
            };
            btnReturn.Click += BtnReturn_Click;

            btnEdit = new Button
            {
                Text = "Редагувати",
                Location = new Point(280, 90),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(33, 150, 243),
                ForeColor = Color.White
            };
            btnEdit.Click += BtnEdit_Click;

            btnDelete = new Button
            {
                Text = "Видалити",
                Location = new Point(390, 90),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(244, 67, 54),
                ForeColor = Color.White
            };
            btnDelete.Click += BtnDelete_Click;

            dgv = new DataGridView
            {
                Location = new Point(20, 130),
                Size = new Size(1150, 420),
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            this.Controls.AddRange(new Control[] { lblTitle, chkShowReturned, btnAdd, btnReturn, btnEdit, btnDelete, dgv });
        }

        private void LoadData()
        {
            string query = @"SELECT 
                ia.assignment_id AS 'ID',
                CONCAT(s.last_name, ' ', s.first_name) AS 'Учень',
                i.name AS 'Інструмент',
                ia.issue_date AS 'Дата видачі',
                ia.expected_return_date AS 'Очікуваний повернення',
                ia.return_date AS 'Повернуто',
                CONCAT(t.last_name, ' ', t.first_name) AS 'Видав викладач',
                CASE 
                    WHEN ia.return_date IS NOT NULL THEN 'Повернуто'
                    WHEN ia.expected_return_date < CURDATE() THEN 'Прострочено'
                    ELSE 'Активно'
                END AS 'Статус'
            FROM instrument_assignments ia
            JOIN students s ON ia.student_id = s.student_id
            JOIN instruments i ON ia.instrument_id = i.instrument_id
            LEFT JOIN teachers t ON ia.teacher_id = t.teacher_id
            WHERE 1=1";

            if (!chkShowReturned.Checked)
            {
                query += " AND ia.return_date IS NULL";
            }

            query += " ORDER BY ia.issue_date DESC";

            dgv.DataSource = DatabaseHelper.ExecuteQuery(query);

            if (dgv.Columns.Contains("Статус"))
            {
                dgv.Columns["Статус"].DefaultCellStyle.Font = new Font(dgv.Font, FontStyle.Bold);
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            InstrumentAssignmentEditForm form = new InstrumentAssignmentEditForm();
            if (form.ShowDialog() == DialogResult.OK) LoadData();
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Оберіть запис!", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int id = Convert.ToInt32(dgv.SelectedRows[0].Cells["ID"].Value);
            InstrumentAssignmentEditForm form = new InstrumentAssignmentEditForm(id);
            if (form.ShowDialog() == DialogResult.OK) LoadData();
        }

        private void BtnReturn_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Оберіть запис!", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (dgv.SelectedRows[0].Cells["Повернуто"].Value != DBNull.Value)
            {
                MessageBox.Show("Інструмент вже повернуто!", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Підтвердити повернення інструменту?", "Підтвердження",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                int id = Convert.ToInt32(dgv.SelectedRows[0].Cells["ID"].Value);
                string query = "UPDATE instrument_assignments SET return_date = @date WHERE assignment_id = @id";
                MySqlParameter[] parameters = {
                    new MySqlParameter("@date", DateTime.Now.Date),
                    new MySqlParameter("@id", id)
                };

                if (DatabaseHelper.ExecuteNonQuery(query, parameters) > 0)
                {
                    MessageBox.Show("Інструмент повернуто!", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadData();
                }
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Оберіть запис!", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Видалити запис про видачу?", "Підтвердження",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                int id = Convert.ToInt32(dgv.SelectedRows[0].Cells["ID"].Value);
                if (DatabaseHelper.ExecuteNonQuery("DELETE FROM instrument_assignments WHERE assignment_id = @id",
                    new[] { new MySqlParameter("@id", id) }) > 0)
                {
                    MessageBox.Show("Запис видалено!", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadData();
                }
            }
        }
    }

    public partial class InstrumentAssignmentEditForm : Form
    {
        private int? assignmentId;
        private ComboBox cmbStudent, cmbInstrument, cmbTeacher;
        private DateTimePicker dtpIssue, dtpExpectedReturn, dtpReturn;
        private TextBox txtNotes;
        private CheckBox chkReturned;
        private Button btnSave, btnCancel;

        public InstrumentAssignmentEditForm(int? id = null)
        {
            assignmentId = id;
            InitializeComponent();
            LoadComboBoxes();
            if (assignmentId.HasValue) LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = assignmentId.HasValue ? "Редагування видачі" : "Видача інструменту";
            this.Size = new Size(500, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            int y = 20;

            Label lblStudent = new Label { Text = "Учень:", Location = new Point(20, y), AutoSize = true };
            cmbStudent = new ComboBox { Location = new Point(180, y), Size = new Size(280, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            this.Controls.AddRange(new Control[] { lblStudent, cmbStudent });
            y += 40;

            Label lblInstrument = new Label { Text = "Інструмент:", Location = new Point(20, y), AutoSize = true };
            cmbInstrument = new ComboBox { Location = new Point(180, y), Size = new Size(280, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            this.Controls.AddRange(new Control[] { lblInstrument, cmbInstrument });
            y += 40;

            Label lblTeacher = new Label { Text = "Викладач (видав):", Location = new Point(20, y), AutoSize = true };
            cmbTeacher = new ComboBox { Location = new Point(180, y), Size = new Size(280, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            this.Controls.AddRange(new Control[] { lblTeacher, cmbTeacher });
            y += 40;

            Label lblIssue = new Label { Text = "Дата видачі:", Location = new Point(20, y), AutoSize = true };
            dtpIssue = new DateTimePicker { Location = new Point(180, y), Size = new Size(280, 25), Format = DateTimePickerFormat.Short };
            this.Controls.AddRange(new Control[] { lblIssue, dtpIssue });
            y += 40;

            Label lblExpected = new Label { Text = "Очікуване повернення:", Location = new Point(20, y), AutoSize = true };
            dtpExpectedReturn = new DateTimePicker { Location = new Point(180, y), Size = new Size(280, 25), Format = DateTimePickerFormat.Short };
            this.Controls.AddRange(new Control[] { lblExpected, dtpExpectedReturn });
            y += 40;

            chkReturned = new CheckBox { Text = "Інструмент повернуто", Location = new Point(20, y), AutoSize = true };
            chkReturned.CheckedChanged += (s, e) => dtpReturn.Enabled = chkReturned.Checked;
            this.Controls.Add(chkReturned);
            y += 30;

            Label lblReturn = new Label { Text = "Дата повернення:", Location = new Point(20, y), AutoSize = true };
            dtpReturn = new DateTimePicker { Location = new Point(180, y), Size = new Size(280, 25), Format = DateTimePickerFormat.Short, Enabled = false };
            this.Controls.AddRange(new Control[] { lblReturn, dtpReturn });
            y += 40;

            Label lblNotes = new Label { Text = "Примітки:", Location = new Point(20, y), AutoSize = true };
            txtNotes = new TextBox { Location = new Point(180, y), Size = new Size(280, 80), Multiline = true };
            this.Controls.AddRange(new Control[] { lblNotes, txtNotes });
            y += 95;

            btnSave = new Button
            {
                Text = "Зберегти",
                Location = new Point(180, y),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                DialogResult = DialogResult.OK
            };
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button
            {
                Text = "Скасувати",
                Location = new Point(310, y),
                Size = new Size(120, 35),
                DialogResult = DialogResult.Cancel
            };

            this.Controls.AddRange(new Control[] { btnSave, btnCancel });
        }

        private void LoadComboBoxes()
        {
            DataTable dtStudents = DatabaseHelper.ExecuteQuery(
                "SELECT student_id, CONCAT(last_name, ' ', first_name, ' (', class, ')') AS name FROM students ORDER BY last_name");
            cmbStudent.DisplayMember = "name";
            cmbStudent.ValueMember = "student_id";
            cmbStudent.DataSource = dtStudents;

            DataTable dtInstruments = DatabaseHelper.ExecuteQuery(
                "SELECT instrument_id, CONCAT(name, ' - ', brand, ' ', model) AS name FROM instruments ORDER BY name");
            cmbInstrument.DisplayMember = "name";
            cmbInstrument.ValueMember = "instrument_id";
            cmbInstrument.DataSource = dtInstruments;

            DataTable dtTeachers = DatabaseHelper.ExecuteQuery(
                "SELECT teacher_id, CONCAT(last_name, ' ', first_name) AS name FROM teachers ORDER BY last_name");
            cmbTeacher.DisplayMember = "name";
            cmbTeacher.ValueMember = "teacher_id";
            cmbTeacher.DataSource = dtTeachers;
        }

        private void LoadData()
        {
            DataTable dt = DatabaseHelper.ExecuteQuery(
                "SELECT * FROM instrument_assignments WHERE assignment_id = @id",
                new[] { new MySqlParameter("@id", assignmentId) });

            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                cmbStudent.SelectedValue = row["student_id"];
                cmbInstrument.SelectedValue = row["instrument_id"];
                cmbTeacher.SelectedValue = row["teacher_id"];
                dtpIssue.Value = Convert.ToDateTime(row["issue_date"]);
                if (row["expected_return_date"] != DBNull.Value)
                    dtpExpectedReturn.Value = Convert.ToDateTime(row["expected_return_date"]);

                if (row["return_date"] != DBNull.Value)
                {
                    chkReturned.Checked = true;
                    dtpReturn.Value = Convert.ToDateTime(row["return_date"]);
                }

                txtNotes.Text = row["notes"].ToString();
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (cmbStudent.SelectedValue == null || cmbInstrument.SelectedValue == null)
            {
                MessageBox.Show("Оберіть учня та інструмент!", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.None;
                return;
            }

            string query = assignmentId.HasValue
                ? @"UPDATE instrument_assignments SET student_id=@sid, instrument_id=@iid, teacher_id=@tid,
                   issue_date=@idate, expected_return_date=@edate, return_date=@rdate, notes=@notes WHERE assignment_id=@id"
                : @"INSERT INTO instrument_assignments (student_id, instrument_id, teacher_id, issue_date, expected_return_date, return_date, notes)
                   VALUES (@sid, @iid, @tid, @idate, @edate, @rdate, @notes)";

            var p = new[] {
                new MySqlParameter("@sid", cmbStudent.SelectedValue),
                new MySqlParameter("@iid", cmbInstrument.SelectedValue),
                new MySqlParameter("@tid", cmbTeacher.SelectedValue ?? (object)DBNull.Value),
                new MySqlParameter("@idate", dtpIssue.Value.Date),
                new MySqlParameter("@edate", dtpExpectedReturn.Value.Date),
                new MySqlParameter("@rdate", chkReturned.Checked ? (object)dtpReturn.Value.Date : DBNull.Value),
                new MySqlParameter("@notes", txtNotes.Text)
            };

            if (assignmentId.HasValue)
            {
                Array.Resize(ref p, p.Length + 1);
                p[p.Length - 1] = new MySqlParameter("@id", assignmentId);
            }

            if (DatabaseHelper.ExecuteNonQuery(query, p) > 0)
                MessageBox.Show("Збережено!", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                this.DialogResult = DialogResult.None;
        }
    }
}