using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace MusicSchoolApp
{
    public partial class SubjectsForm : Form
    {
        private DataGridView dgv;
        private Button btnAdd, btnEdit, btnDelete;
        private Label lblAccessInfo;

        public SubjectsForm()
        {
            InitializeComponent();
            ApplyAccessControl();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(800, 550);

            Label lbl = new Label
            {
                Text = "Навчальні предмети",
                Font = new Font("Arial", 16, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };

            lblAccessInfo = new Label
            {
                Text = "ℹ️ Staff: Ви можете тільки переглядати предмети. Створення/видалення - тільки для адміністраторів.",
                ForeColor = Color.DarkOrange,
                Location = new Point(20, 50),
                Size = new Size(750, 20),
                Visible = false
            };

            btnAdd = new Button
            {
                Text = "Додати",
                Location = new Point(20, 85),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White
            };
            btnAdd.Click += BtnAdd_Click;

            btnEdit = new Button
            {
                Text = "Редагувати",
                Location = new Point(130, 85),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(33, 150, 243),
                ForeColor = Color.White
            };
            btnEdit.Click += BtnEdit_Click;

            btnDelete = new Button
            {
                Text = "Видалити",
                Location = new Point(240, 85),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(244, 67, 54),
                ForeColor = Color.White
            };
            btnDelete.Click += BtnDelete_Click;

            dgv = new DataGridView
            {
                Location = new Point(20, 125),
                Size = new Size(750, 375),
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            this.Controls.AddRange(new Control[] { lbl, lblAccessInfo, btnAdd, btnEdit, btnDelete, dgv });
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

        private void LoadData()
        {
            dgv.DataSource = DatabaseHelper.ExecuteQuery(
                "SELECT subject_id AS 'ID', name AS 'Назва', description AS 'Опис' FROM subjects ORDER BY name");
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (!UserSession.CanAddSubjects)
            {
                MessageBox.Show("Тільки адміністратори можуть додавати предмети!\n\n" +
                    "Створення предметів впливає на весь навчальний процес.",
                    "Доступ заборонено", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SubjectEditForm f = new SubjectEditForm();
            if (f.ShowDialog() == DialogResult.OK) LoadData();
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (!UserSession.CanEditSubjects)
            {
                MessageBox.Show("Тільки адміністратори можуть редагувати предмети!",
                    "Доступ заборонено", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (dgv.SelectedRows.Count == 0) { MessageBox.Show("Оберіть предмет!"); return; }
            SubjectEditForm f = new SubjectEditForm(Convert.ToInt32(dgv.SelectedRows[0].Cells["ID"].Value));
            if (f.ShowDialog() == DialogResult.OK) LoadData();
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (!UserSession.CanDeleteSubjects)
            {
                MessageBox.Show("Тільки адміністратори можуть видаляти предмети!\n\n" +
                    "Видалення предмету впливає на розклад та записи учнів.",
                    "Доступ заборонено", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (dgv.SelectedRows.Count == 0) { MessageBox.Show("Оберіть предмет!"); return; }
            if (MessageBox.Show("Видалити предмет? Це вплине на всі записи учнів!", "Підтвердження",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                DatabaseHelper.ExecuteNonQuery("DELETE FROM subjects WHERE subject_id = @id",
                    new[] { new MySqlParameter("@id", dgv.SelectedRows[0].Cells["ID"].Value) });
                LoadData();
            }
        }
    }

    public partial class SubjectEditForm : Form
    {
        private int? id;
        private TextBox txtName, txtDesc;

        public SubjectEditForm(int? subjectId = null)
        {
            id = subjectId;
            InitializeComponent();
            if (id.HasValue) LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = id.HasValue ? "Редагування предмету" : "Додавання предмету";
            this.Size = new Size(450, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            Label lblName = new Label { Text = "Назва:", Location = new Point(20, 20), AutoSize = true };
            txtName = new TextBox { Location = new Point(120, 18), Size = new Size(300, 25) };

            Label lblDesc = new Label { Text = "Опис:", Location = new Point(20, 60), AutoSize = true };
            txtDesc = new TextBox { Location = new Point(120, 58), Size = new Size(300, 100), Multiline = true };

            Button btnSave = new Button
            {
                Text = "Зберегти",
                Location = new Point(120, 180),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                DialogResult = DialogResult.OK
            };
            btnSave.Click += BtnSave_Click;

            Button btnCancel = new Button
            {
                Text = "Скасувати",
                Location = new Point(230, 180),
                Size = new Size(100, 35),
                DialogResult = DialogResult.Cancel
            };

            this.Controls.AddRange(new Control[] { lblName, txtName, lblDesc, txtDesc, btnSave, btnCancel });
        }

        private void LoadData()
        {
            DataTable dt = DatabaseHelper.ExecuteQuery("SELECT * FROM subjects WHERE subject_id = @id",
                new[] { new MySqlParameter("@id", id) });
            if (dt.Rows.Count > 0)
            {
                txtName.Text = dt.Rows[0]["name"].ToString();
                txtDesc.Text = dt.Rows[0]["description"].ToString();
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text)) { MessageBox.Show("Введіть назву!"); this.DialogResult = DialogResult.None; return; }

            string query = id.HasValue
                ? "UPDATE subjects SET name=@name, description=@desc WHERE subject_id=@id"
                : "INSERT INTO subjects (name, description) VALUES (@name, @desc)";

            var p = new[] { new MySqlParameter("@name", txtName.Text), new MySqlParameter("@desc", txtDesc.Text) };
            if (id.HasValue)
            {
                Array.Resize(ref p, 3);
                p[2] = new MySqlParameter("@id", id);
            }

            DatabaseHelper.ExecuteNonQuery(query, p);
            MessageBox.Show("Збережено!");
        }
    }
}