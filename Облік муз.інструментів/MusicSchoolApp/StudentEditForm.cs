using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace MusicSchoolApp
{
    public partial class StudentEditForm : Form
    {
        private int? studentId;
        private TextBox txtFirstName, txtLastName, txtPhone, txtEmail, txtAddress, txtNotes, txtClass;
        private DateTimePicker dtpBirthDate;
        private NumericUpDown nudEnrollmentYear;
        private Button btnSave, btnCancel;

        public StudentEditForm(int? id = null)
        {
            studentId = id;
            InitializeComponents();
            if (studentId.HasValue)
            {
                LoadStudentData();
            }
        }

        private void InitializeComponents()
        {
            this.Text = studentId.HasValue ? "Редагування учня" : "Додавання учня";
            this.Size = new Size(500, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            int y = 20;
            int labelX = 20;
            int controlX = 150;
            int controlWidth = 300;

            AddField("Прізвище:", ref txtLastName, ref y, labelX, controlX, controlWidth);
            AddField("Ім'я:", ref txtFirstName, ref y, labelX, controlX, controlWidth);

            Label lblBirthDate = new Label { Text = "Дата народження:", Location = new Point(labelX, y), AutoSize = true };
            dtpBirthDate = new DateTimePicker { Location = new Point(controlX, y), Size = new Size(controlWidth, 25), Format = DateTimePickerFormat.Short };
            this.Controls.AddRange(new Control[] { lblBirthDate, dtpBirthDate });
            y += 40;

            AddField("Клас:", ref txtClass, ref y, labelX, controlX, controlWidth);

            Label lblEnrollment = new Label { Text = "Рік вступу:", Location = new Point(labelX, y), AutoSize = true };
            nudEnrollmentYear = new NumericUpDown
            {
                Location = new Point(controlX, y),
                Size = new Size(controlWidth, 25),
                Minimum = 2000,
                Maximum = 2100,
                Value = DateTime.Now.Year
            };
            this.Controls.AddRange(new Control[] { lblEnrollment, nudEnrollmentYear });
            y += 40;

            AddField("Телефон:", ref txtPhone, ref y, labelX, controlX, controlWidth);
            AddField("Email:", ref txtEmail, ref y, labelX, controlX, controlWidth);

            Label lblAddress = new Label { Text = "Адреса:", Location = new Point(labelX, y), AutoSize = true };
            txtAddress = new TextBox { Location = new Point(controlX, y), Size = new Size(controlWidth, 60), Multiline = true };
            this.Controls.AddRange(new Control[] { lblAddress, txtAddress });
            y += 75;

            Label lblNotes = new Label { Text = "Примітки:", Location = new Point(labelX, y), AutoSize = true };
            txtNotes = new TextBox { Location = new Point(controlX, y), Size = new Size(controlWidth, 60), Multiline = true };
            this.Controls.AddRange(new Control[] { lblNotes, txtNotes });
            y += 75;

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

        private void AddField(string labelText, ref TextBox textBox, ref int y, int labelX, int controlX, int controlWidth)
        {
            Label label = new Label { Text = labelText, Location = new Point(labelX, y), AutoSize = true };
            textBox = new TextBox { Location = new Point(controlX, y), Size = new Size(controlWidth, 25) };
            this.Controls.AddRange(new Control[] { label, textBox });
            y += 40;
        }

        private void LoadStudentData()
        {
            string query = "SELECT * FROM students WHERE student_id = @id";
            MySqlParameter[] parameters = { new MySqlParameter("@id", studentId) };
            DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters);

            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                txtFirstName.Text = row["first_name"].ToString();
                txtLastName.Text = row["last_name"].ToString();
                if (row["birth_date"] != DBNull.Value)
                    dtpBirthDate.Value = Convert.ToDateTime(row["birth_date"]);
                txtClass.Text = row["class"].ToString();
                if (row["enrollment_year"] != DBNull.Value)
                    nudEnrollmentYear.Value = Convert.ToInt32(row["enrollment_year"]);
                txtPhone.Text = row["phone"].ToString();
                txtEmail.Text = row["email"].ToString();
                txtAddress.Text = row["address"].ToString();
                txtNotes.Text = row["notes"].ToString();
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFirstName.Text) || string.IsNullOrWhiteSpace(txtLastName.Text))
            {
                MessageBox.Show("Будь ласка, заповніть ім'я та прізвище!", "Помилка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.None;
                return;
            }

            string query;
            if (studentId.HasValue)
            {
                query = @"UPDATE students SET first_name=@fn, last_name=@ln, birth_date=@bd, 
                         class=@class, enrollment_year=@ey, phone=@phone, email=@email, 
                         address=@addr, notes=@notes WHERE student_id=@id";
            }
            else
            {
                query = @"INSERT INTO students (first_name, last_name, birth_date, class, 
                         enrollment_year, phone, email, address, notes) 
                         VALUES (@fn, @ln, @bd, @class, @ey, @phone, @email, @addr, @notes)";
            }

            MySqlParameter[] parameters = {
                new MySqlParameter("@fn", txtFirstName.Text),
                new MySqlParameter("@ln", txtLastName.Text),
                new MySqlParameter("@bd", dtpBirthDate.Value.Date),
                new MySqlParameter("@class", txtClass.Text),
                new MySqlParameter("@ey", nudEnrollmentYear.Value),
                new MySqlParameter("@phone", txtPhone.Text),
                new MySqlParameter("@email", txtEmail.Text),
                new MySqlParameter("@addr", txtAddress.Text),
                new MySqlParameter("@notes", txtNotes.Text)
            };

            if (studentId.HasValue)
            {
                Array.Resize(ref parameters, parameters.Length + 1);
                parameters[parameters.Length - 1] = new MySqlParameter("@id", studentId);
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