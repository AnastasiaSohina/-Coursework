using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace MusicSchoolApp
{
    public partial class ScheduleForm : Form
    {
        private DataGridView dgvSchedule;
        private Button btnAdd, btnEdit, btnDelete, btnRefresh;
        private DateTimePicker dtpFilter;
        private ComboBox cmbTeacher, cmbSubject, cmbRoom;
        private CheckBox chkFilterByDate;
        private Panel filterPanel;

        public ScheduleForm()
        {
            InitializeComponents();
            this.Load += (s, e) => LoadComboBoxes();
            this.Load += (s, e) => LoadSchedule();
        }

        private void InitializeComponents()
        {
            this.Size = new Size(1200, 700);
            this.BackColor = Color.White;

            filterPanel = new Panel
            {
                Location = new Point(10, 10),
                Size = new Size(1160, 100),
                BackColor = Color.FromArgb(240, 240, 240),
                BorderStyle = BorderStyle.FixedSingle
            };

            chkFilterByDate = new CheckBox
            {
                Text = "Фільтр по даті:",
                Location = new Point(15, 15),
                AutoSize = true
            };
            chkFilterByDate.CheckedChanged += (s, e) =>
            {
                dtpFilter.Enabled = chkFilterByDate.Checked;
                LoadSchedule();
            };

            dtpFilter = new DateTimePicker
            {
                Location = new Point(140, 12),
                Width = 150,
                Format = DateTimePickerFormat.Short,
                Enabled = false
            };
            dtpFilter.ValueChanged += (s, e) => { if (chkFilterByDate.Checked) LoadSchedule(); };

            Label lblTeacher = new Label { Text = "Викладач:", Location = new Point(310, 15), AutoSize = true };
            cmbTeacher = new ComboBox
            {
                Location = new Point(400, 12),
                Width = 220,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbTeacher.SelectedIndexChanged += (s, e) => LoadSchedule();

            Label lblSubject = new Label { Text = "Предмет:", Location = new Point(640, 15), AutoSize = true };
            cmbSubject = new ComboBox
            {
                Location = new Point(720, 12),
                Width = 220,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbSubject.SelectedIndexChanged += (s, e) => LoadSchedule();

            Label lblRoom = new Label { Text = "Аудиторія:", Location = new Point(15, 50), AutoSize = true };
            cmbRoom = new ComboBox
            {
                Location = new Point(100, 47),
                Width = 190,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbRoom.SelectedIndexChanged += (s, e) => LoadSchedule();

            filterPanel.Controls.AddRange(new Control[] {
                chkFilterByDate, dtpFilter, lblTeacher, cmbTeacher,
                lblSubject, cmbSubject, lblRoom, cmbRoom
            });

            dgvSchedule = new DataGridView
            {
                Location = new Point(10, 120),
                Size = new Size(1160, 470),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                BackgroundColor = Color.White,
                RowHeadersVisible = false
            };

            dgvSchedule.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(63, 81, 181);
            dgvSchedule.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvSchedule.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 10, FontStyle.Bold);
            dgvSchedule.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);

            btnAdd = new Button
            {
                Text = "Додати заняття",
                Location = new Point(10, 600),
                Size = new Size(140, 35),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnAdd.Click += BtnAdd_Click;

            btnEdit = new Button
            {
                Text = "Редагувати",
                Location = new Point(160, 600),
                Size = new Size(140, 35),
                BackColor = Color.FromArgb(33, 150, 243),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnEdit.Click += BtnEdit_Click;

            btnDelete = new Button
            {
                Text = "Видалити",
                Location = new Point(310, 600),
                Size = new Size(140, 35),
                BackColor = Color.FromArgb(244, 67, 54),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnDelete.Click += BtnDelete_Click;

            btnRefresh = new Button
            {
                Text = "Оновити",
                Location = new Point(460, 600),
                Size = new Size(140, 35),
                BackColor = Color.FromArgb(158, 158, 158),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnRefresh.Click += (s, e) => LoadSchedule();

            this.Controls.AddRange(new Control[] {
                filterPanel, dgvSchedule, btnAdd, btnEdit, btnDelete, btnRefresh
            });
        }

        private void LoadComboBoxes()
        {
            DataTable teachers = DatabaseHelper.ExecuteQuery(
                "SELECT teacher_id, CONCAT(last_name, ' ', first_name) AS full_name FROM teachers ORDER BY last_name");
            cmbTeacher.Items.Add(new ComboBoxItem { Text = "Всі викладачі", Value = 0 });
            foreach (DataRow row in teachers.Rows)
            {
                cmbTeacher.Items.Add(new ComboBoxItem
                {
                    Text = row["full_name"].ToString(),
                    Value = Convert.ToInt32(row["teacher_id"])
                });
            }
            cmbTeacher.DisplayMember = "Text";
            cmbTeacher.ValueMember = "Value";
            cmbTeacher.SelectedIndex = 0;

            DataTable subjects = DatabaseHelper.ExecuteQuery(
                "SELECT subject_id, name FROM subjects ORDER BY name");
            cmbSubject.Items.Add(new ComboBoxItem { Text = "Всі предмети", Value = 0 });
            foreach (DataRow row in subjects.Rows)
            {
                cmbSubject.Items.Add(new ComboBoxItem
                {
                    Text = row["name"].ToString(),
                    Value = Convert.ToInt32(row["subject_id"])
                });
            }
            cmbSubject.DisplayMember = "Text";
            cmbSubject.ValueMember = "Value";
            cmbSubject.SelectedIndex = 0;

            DataTable rooms = DatabaseHelper.ExecuteQuery(
                "SELECT room_id, name FROM rooms ORDER BY name");
            cmbRoom.Items.Add(new ComboBoxItem { Text = "Всі аудиторії", Value = 0 });
            foreach (DataRow row in rooms.Rows)
            {
                cmbRoom.Items.Add(new ComboBoxItem
                {
                    Text = row["name"].ToString(),
                    Value = Convert.ToInt32(row["room_id"])
                });
            }
            cmbRoom.DisplayMember = "Text";
            cmbRoom.ValueMember = "Value";
            cmbRoom.SelectedIndex = 0;
        }

        private void LoadSchedule()
        {
            string query = @"
                SELECT 
                    s.schedule_id AS 'ID',
                    s.lesson_date AS 'Дата',
                    s.start_time AS 'Початок',
                    s.end_time AS 'Кінець',
                    sub.name AS 'Предмет',
                    CONCAT(t.last_name, ' ', t.first_name) AS 'Викладач',
                    r.name AS 'Аудиторія'
                FROM schedule s
                INNER JOIN subjects sub ON s.subject_id = sub.subject_id
                INNER JOIN teachers t ON s.teacher_id = t.teacher_id
                INNER JOIN rooms r ON s.room_id = r.room_id
                WHERE 1=1";

            if (chkFilterByDate.Checked)
            {
                query += $" AND s.lesson_date = '{dtpFilter.Value:yyyy-MM-dd}'";
            }

            if (cmbTeacher.SelectedItem != null)
            {
                var teacherItem = (ComboBoxItem)cmbTeacher.SelectedItem;
                if ((int)teacherItem.Value > 0)
                    query += $" AND s.teacher_id = {teacherItem.Value}";
            }

            if (cmbSubject.SelectedItem != null)
            {
                var subjectItem = (ComboBoxItem)cmbSubject.SelectedItem;
                if ((int)subjectItem.Value > 0)
                    query += $" AND s.subject_id = {subjectItem.Value}";
            }

            if (cmbRoom.SelectedItem != null)
            {
                var roomItem = (ComboBoxItem)cmbRoom.SelectedItem;
                if ((int)roomItem.Value > 0)
                    query += $" AND s.room_id = {roomItem.Value}";
            }

            query += " ORDER BY s.lesson_date, s.start_time";

            DataTable dt = DatabaseHelper.ExecuteQuery(query);
            dgvSchedule.DataSource = dt;

            if (dgvSchedule.Columns.Count > 0)
            {
                dgvSchedule.Columns["ID"].Visible = false;
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            ScheduleEditForm editForm = new ScheduleEditForm();
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                LoadSchedule();
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgvSchedule.SelectedRows.Count == 0)
            {
                MessageBox.Show("Оберіть заняття для редагування!", "Увага",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int scheduleId = Convert.ToInt32(dgvSchedule.SelectedRows[0].Cells["ID"].Value);
            ScheduleEditForm editForm = new ScheduleEditForm(scheduleId);
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                LoadSchedule();
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvSchedule.SelectedRows.Count == 0)
            {
                MessageBox.Show("Оберіть заняття для видалення!", "Увага",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show("Ви впевнені, що хочете видалити це заняття?",
                "Підтвердження", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                int scheduleId = Convert.ToInt32(dgvSchedule.SelectedRows[0].Cells["ID"].Value);

                string query = "DELETE FROM schedule WHERE schedule_id = @id";
                MySqlParameter[] parameters = { new MySqlParameter("@id", scheduleId) };

                if (DatabaseHelper.ExecuteNonQuery(query, parameters) > 0)
                {
                    MessageBox.Show("Заняття успішно видалено!", "Успіх",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadSchedule();
                }
            }
        }
    }

    public class ComboBoxItem
    {
        public string Text { get; set; }
        public object Value { get; set; }
    }

    public class ScheduleEditForm : Form
    {
        private int? scheduleId;
        private DateTimePicker dtpDate, dtpStartTime, dtpEndTime;
        private ComboBox cmbSubject, cmbTeacher, cmbRoom;
        private Button btnSave, btnCancel;

        public ScheduleEditForm(int? id = null)
        {
            scheduleId = id;
            InitializeComponents();
            LoadComboBoxes();

            if (scheduleId.HasValue)
            {
                LoadScheduleData();
            }
        }

        private void InitializeComponents()
        {
            this.Text = scheduleId.HasValue ? "Редагування заняття" : "Додавання заняття";
            this.Size = new Size(500, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            int y = 20;
            int labelX = 20;
            int controlX = 150;
            int controlWidth = 300;

            Label lblDate = new Label { Text = "Дата:", Location = new Point(labelX, y), AutoSize = true };
            dtpDate = new DateTimePicker
            {
                Location = new Point(controlX, y),
                Size = new Size(controlWidth, 25),
                Format = DateTimePickerFormat.Short
            };
            this.Controls.AddRange(new Control[] { lblDate, dtpDate });
            y += 40;

            Label lblStartTime = new Label { Text = "Початок:", Location = new Point(labelX, y), AutoSize = true };
            dtpStartTime = new DateTimePicker
            {
                Location = new Point(controlX, y),
                Size = new Size(controlWidth, 25),
                Format = DateTimePickerFormat.Time,
                ShowUpDown = true
            };
            this.Controls.AddRange(new Control[] { lblStartTime, dtpStartTime });
            y += 40;

            Label lblEndTime = new Label { Text = "Кінець:", Location = new Point(labelX, y), AutoSize = true };
            dtpEndTime = new DateTimePicker
            {
                Location = new Point(controlX, y),
                Size = new Size(controlWidth, 25),
                Format = DateTimePickerFormat.Time,
                ShowUpDown = true
            };
            this.Controls.AddRange(new Control[] { lblEndTime, dtpEndTime });
            y += 40;

            Label lblSubject = new Label { Text = "Предмет:", Location = new Point(labelX, y), AutoSize = true };
            cmbSubject = new ComboBox
            {
                Location = new Point(controlX, y),
                Size = new Size(controlWidth, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            this.Controls.AddRange(new Control[] { lblSubject, cmbSubject });
            y += 40;

            Label lblTeacher = new Label { Text = "Викладач:", Location = new Point(labelX, y), AutoSize = true };
            cmbTeacher = new ComboBox
            {
                Location = new Point(controlX, y),
                Size = new Size(controlWidth, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            this.Controls.AddRange(new Control[] { lblTeacher, cmbTeacher });
            y += 40;

            Label lblRoom = new Label { Text = "Аудиторія:", Location = new Point(labelX, y), AutoSize = true };
            cmbRoom = new ComboBox
            {
                Location = new Point(controlX, y),
                Size = new Size(controlWidth, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            this.Controls.AddRange(new Control[] { lblRoom, cmbRoom });
            y += 50;

            btnSave = new Button
            {
                Text = "Зберегти",
                Location = new Point(150, y),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                DialogResult = DialogResult.OK
            };
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button
            {
                Text = "Скасувати",
                Location = new Point(280, y),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(158, 158, 158),
                ForeColor = Color.White,
                DialogResult = DialogResult.Cancel
            };

            this.Controls.AddRange(new Control[] { btnSave, btnCancel });
        }

        private void LoadComboBoxes()
        {
            DataTable subjects = DatabaseHelper.ExecuteQuery(
                "SELECT subject_id, name FROM subjects ORDER BY name");
            cmbSubject.DisplayMember = "name";
            cmbSubject.ValueMember = "subject_id";
            cmbSubject.DataSource = subjects;

            DataTable teachers = DatabaseHelper.ExecuteQuery(
                "SELECT teacher_id, CONCAT(last_name, ' ', first_name) AS full_name FROM teachers ORDER BY last_name");
            cmbTeacher.DisplayMember = "full_name";
            cmbTeacher.ValueMember = "teacher_id";
            cmbTeacher.DataSource = teachers;

            DataTable rooms = DatabaseHelper.ExecuteQuery(
                "SELECT room_id, name FROM rooms ORDER BY name");
            cmbRoom.DisplayMember = "name";
            cmbRoom.ValueMember = "room_id";
            cmbRoom.DataSource = rooms;
        }

        private void LoadScheduleData()
        {
            string query = "SELECT * FROM schedule WHERE schedule_id = @id";
            MySqlParameter[] parameters = { new MySqlParameter("@id", scheduleId) };

            DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters);

            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                dtpDate.Value = Convert.ToDateTime(row["lesson_date"]);
                dtpStartTime.Value = DateTime.Today.Add((TimeSpan)row["start_time"]);
                dtpEndTime.Value = DateTime.Today.Add((TimeSpan)row["end_time"]);
                cmbSubject.SelectedValue = row["subject_id"];
                cmbTeacher.SelectedValue = row["teacher_id"];
                cmbRoom.SelectedValue = row["room_id"];
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (cmbSubject.SelectedValue == null || cmbTeacher.SelectedValue == null || cmbRoom.SelectedValue == null)
            {
                MessageBox.Show("Будь ласка, заповніть всі поля!", "Помилка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.None;
                return;
            }

            if (dtpEndTime.Value <= dtpStartTime.Value)
            {
                MessageBox.Show("Час закінчення має бути пізніше часу початку!", "Помилка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.None;
                return;
            }

            string query;
            MySqlParameter[] parameters;

            if (scheduleId.HasValue)
            {
                query = @"UPDATE schedule SET lesson_date=@date, start_time=@start, end_time=@end,
                         subject_id=@subject, teacher_id=@teacher, room_id=@room
                         WHERE schedule_id=@id";

                parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@date", dtpDate.Value.Date),
                    new MySqlParameter("@start", dtpStartTime.Value.TimeOfDay),
                    new MySqlParameter("@end", dtpEndTime.Value.TimeOfDay),
                    new MySqlParameter("@subject", cmbSubject.SelectedValue),
                    new MySqlParameter("@teacher", cmbTeacher.SelectedValue),
                    new MySqlParameter("@room", cmbRoom.SelectedValue),
                    new MySqlParameter("@id", scheduleId.Value)
                };
            }
            else
            {
                query = @"INSERT INTO schedule (lesson_date, start_time, end_time, subject_id, teacher_id, room_id)
                         VALUES (@date, @start, @end, @subject, @teacher, @room)";

                parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@date", dtpDate.Value.Date),
                    new MySqlParameter("@start", dtpStartTime.Value.TimeOfDay),
                    new MySqlParameter("@end", dtpEndTime.Value.TimeOfDay),
                    new MySqlParameter("@subject", cmbSubject.SelectedValue),
                    new MySqlParameter("@teacher", cmbTeacher.SelectedValue),
                    new MySqlParameter("@room", cmbRoom.SelectedValue)
                };
            }

            if (DatabaseHelper.ExecuteNonQuery(query, parameters) > 0)
            {
                MessageBox.Show("Дані успішно збережено!", "Успіх",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                this.DialogResult = DialogResult.None;
            }
        }
    }
}