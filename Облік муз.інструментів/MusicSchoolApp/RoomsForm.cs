using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace MusicSchoolApp
{
    public partial class RoomsForm : Form
    {
        private DataGridView dgv;
        private Button btnAdd, btnEdit, btnDelete;
        private Label lblAccessInfo;

        public RoomsForm()
        {
            InitializeComponents();
            ApplyAccessControl();
            LoadData();
        }

        private void InitializeComponents()
        {
            this.Size = new Size(850, 550);

            Label lbl = new Label
            {
                Text = "Аудиторії та кабінети",
                Font = new Font("Arial", 16, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };

            lblAccessInfo = new Label
            {
                Text = "ℹ️ Staff: Ви можете редагувати базові дані аудиторій. Створення/видалення - тільки для адміністраторів.",
                ForeColor = Color.DarkBlue,
                Location = new Point(20, 50),
                Size = new Size(800, 20),
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
                Size = new Size(800, 375),
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

                btnDelete.Enabled = false;
                btnDelete.BackColor = Color.Gray;
                btnDelete.Cursor = Cursors.No;

            }
        }

        private void LoadData()
        {
            dgv.DataSource = DatabaseHelper.ExecuteQuery(
                "SELECT room_id AS 'ID', name AS 'Назва', type AS 'Тип', capacity AS 'Місткість', notes AS 'Примітки' FROM rooms ORDER BY name");
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (!UserSession.CanAddRooms)
            {
                MessageBox.Show("Тільки адміністратори можуть додавати нові аудиторії!",
                    "Доступ заборонено", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            RoomEditForm f = new RoomEditForm();
            if (f.ShowDialog() == DialogResult.OK) LoadData();
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (!UserSession.CanEditRooms)
            {
                MessageBox.Show("У вас немає прав для редагування аудиторій!",
                    "Доступ заборонено", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (dgv.SelectedRows.Count == 0) { MessageBox.Show("Оберіть аудиторію!"); return; }
            RoomEditForm f = new RoomEditForm(Convert.ToInt32(dgv.SelectedRows[0].Cells["ID"].Value));
            if (f.ShowDialog() == DialogResult.OK) LoadData();
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (!UserSession.CanDeleteRooms)
            {
                MessageBox.Show("Тільки адміністратори можуть видаляти аудиторії!",
                    "Доступ заборонено", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (dgv.SelectedRows.Count == 0) { MessageBox.Show("Оберіть аудиторію!"); return; }
            if (MessageBox.Show("Видалити аудиторію? Це може вплинути на розклад!", "Підтвердження",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                DatabaseHelper.ExecuteNonQuery("DELETE FROM rooms WHERE room_id = @id",
                    new[] { new MySqlParameter("@id", dgv.SelectedRows[0].Cells["ID"].Value) });
                LoadData();
            }
        }
    }

    public partial class RoomEditForm : Form
    {
        private int? id;
        private TextBox txtName, txtType, txtNotes;
        private NumericUpDown nudCapacity;

        public RoomEditForm(int? roomId = null)
        {
            id = roomId;
            InitializeComponent();
            if (id.HasValue) LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = id.HasValue ? "Редагування аудиторії" : "Додавання аудиторії";
            this.Size = new Size(450, 350);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            int y = 20;
            Label lblName = new Label { Text = "Назва:", Location = new Point(20, y), AutoSize = true };
            txtName = new TextBox { Location = new Point(120, y), Size = new Size(300, 25) };
            this.Controls.AddRange(new Control[] { lblName, txtName });
            y += 40;

            Label lblType = new Label { Text = "Тип:", Location = new Point(20, y), AutoSize = true };
            txtType = new TextBox { Location = new Point(120, y), Size = new Size(300, 25) };
            this.Controls.AddRange(new Control[] { lblType, txtType });
            y += 40;

            Label lblCapacity = new Label { Text = "Місткість:", Location = new Point(20, y), AutoSize = true };
            nudCapacity = new NumericUpDown { Location = new Point(120, y), Size = new Size(300, 25), Maximum = 500 };
            this.Controls.AddRange(new Control[] { lblCapacity, nudCapacity });
            y += 40;

            Label lblNotes = new Label { Text = "Примітки:", Location = new Point(20, y), AutoSize = true };
            txtNotes = new TextBox { Location = new Point(120, y), Size = new Size(300, 80), Multiline = true };
            this.Controls.AddRange(new Control[] { lblNotes, txtNotes });
            y += 95;

            Button btnSave = new Button
            {
                Text = "Зберегти",
                Location = new Point(120, y),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                DialogResult = DialogResult.OK
            };
            btnSave.Click += BtnSave_Click;

            Button btnCancel = new Button { Text = "Скасувати", Location = new Point(230, y), Size = new Size(100, 35), DialogResult = DialogResult.Cancel };
            this.Controls.AddRange(new Control[] { btnSave, btnCancel });
        }

        private void LoadData()
        {
            DataTable dt = DatabaseHelper.ExecuteQuery("SELECT * FROM rooms WHERE room_id = @id", new[] { new MySqlParameter("@id", id) });
            if (dt.Rows.Count > 0)
            {
                txtName.Text = dt.Rows[0]["name"].ToString();
                txtType.Text = dt.Rows[0]["type"].ToString();
                if (dt.Rows[0]["capacity"] != DBNull.Value) nudCapacity.Value = Convert.ToInt32(dt.Rows[0]["capacity"]);
                txtNotes.Text = dt.Rows[0]["notes"].ToString();
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text)) { MessageBox.Show("Введіть назву!"); this.DialogResult = DialogResult.None; return; }

            string query = id.HasValue
                ? "UPDATE rooms SET name=@name, type=@type, capacity=@cap, notes=@notes WHERE room_id=@id"
                : "INSERT INTO rooms (name, type, capacity, notes) VALUES (@name, @type, @cap, @notes)";

            var p = new[] {
                new MySqlParameter("@name", txtName.Text),
                new MySqlParameter("@type", txtType.Text),
                new MySqlParameter("@cap", nudCapacity.Value),
                new MySqlParameter("@notes", txtNotes.Text)
            };

            if (id.HasValue)
            {
                Array.Resize(ref p, 5);
                p[4] = new MySqlParameter("@id", id);
            }

            DatabaseHelper.ExecuteNonQuery(query, p);
            MessageBox.Show("Збережено!");
        }
    }
}