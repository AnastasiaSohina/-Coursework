using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace MusicSchoolApp
{
    public partial class SuppliersForm : Form
    {
        private DataGridView dgv;
        private Button btnAdd, btnEdit, btnDelete;
        private Label lblAccessInfo;

        public SuppliersForm()
        {
            InitializeComponents();
            ApplyAccessControl();
            LoadData();
        }

        private void InitializeComponents()
        {
            this.Size = new Size(950, 550);

            Label lbl = new Label
            {
                Text = "Постачальники інструментів",
                Font = new Font("Arial", 16, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };

            lblAccessInfo = new Label
            {
                Text = "ℹ️ Staff: Ви можете редагувати контактні дані. Створення/видалення постачальників - тільки для адміністраторів.",
                ForeColor = Color.DarkBlue,
                Location = new Point(20, 50),
                Size = new Size(900, 20),
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
                Size = new Size(900, 375),
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
                "SELECT supplier_id AS 'ID', name AS 'Назва', phone AS 'Телефон', email AS 'Email', address AS 'Адреса' FROM suppliers ORDER BY name");
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (!UserSession.CanAddSuppliers)
            {
                MessageBox.Show("Тільки адміністратори можуть додавати нових постачальників!\n\n" +
                    "Постачальниками управляє менеджер з матеріальної частини.",
                    "Доступ заборонено", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SupplierEditForm f = new SupplierEditForm();
            if (f.ShowDialog() == DialogResult.OK) LoadData();
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (!UserSession.CanEditSuppliers)
            {
                MessageBox.Show("У вас немає прав для редагування постачальників!",
                    "Доступ заборонено", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (dgv.SelectedRows.Count == 0) { MessageBox.Show("Оберіть постачальника!"); return; }
            SupplierEditForm f = new SupplierEditForm(Convert.ToInt32(dgv.SelectedRows[0].Cells["ID"].Value));
            if (f.ShowDialog() == DialogResult.OK) LoadData();
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (!UserSession.CanDeleteSuppliers)
            {
                MessageBox.Show("Тільки адміністратори можуть видаляти постачальників!",
                    "Доступ заборонено", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (dgv.SelectedRows.Count == 0) { MessageBox.Show("Оберіть постачальника!"); return; }
            if (MessageBox.Show("Видалити постачальника? Історія закупівель збережеться.", "Підтвердження",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                DatabaseHelper.ExecuteNonQuery("DELETE FROM suppliers WHERE supplier_id = @id",
                    new[] { new MySqlParameter("@id", dgv.SelectedRows[0].Cells["ID"].Value) });
                LoadData();
            }
        }
    }

    public partial class SupplierEditForm : Form
    {
        private int? id;
        private TextBox txtName, txtPhone, txtEmail, txtAddress, txtNotes;

        public SupplierEditForm(int? supplierId = null)
        {
            id = supplierId;
            InitializeComponent();
            if (id.HasValue) LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = id.HasValue ? "Редагування постачальника" : "Додавання постачальника";
            this.Size = new Size(450, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            int y = 20;
            Label lblName = new Label { Text = "Назва:", Location = new Point(20, y), AutoSize = true };
            txtName = new TextBox { Location = new Point(120, y), Size = new Size(300, 25) };
            this.Controls.AddRange(new Control[] { lblName, txtName });
            y += 40;

            Label lblPhone = new Label { Text = "Телефон:", Location = new Point(20, y), AutoSize = true };
            txtPhone = new TextBox { Location = new Point(120, y), Size = new Size(300, 25) };
            this.Controls.AddRange(new Control[] { lblPhone, txtPhone });
            y += 40;

            Label lblEmail = new Label { Text = "Email:", Location = new Point(20, y), AutoSize = true };
            txtEmail = new TextBox { Location = new Point(120, y), Size = new Size(300, 25) };
            this.Controls.AddRange(new Control[] { lblEmail, txtEmail });
            y += 40;

            Label lblAddr = new Label { Text = "Адреса:", Location = new Point(20, y), AutoSize = true };
            txtAddress = new TextBox { Location = new Point(120, y), Size = new Size(300, 60), Multiline = true };
            this.Controls.AddRange(new Control[] { lblAddr, txtAddress });
            y += 75;

            Label lblNotes = new Label { Text = "Примітки:", Location = new Point(20, y), AutoSize = true };
            txtNotes = new TextBox { Location = new Point(120, y), Size = new Size(300, 60), Multiline = true };
            this.Controls.AddRange(new Control[] { lblNotes, txtNotes });
            y += 75;

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
            DataTable dt = DatabaseHelper.ExecuteQuery("SELECT * FROM suppliers WHERE supplier_id = @id", new[] { new MySqlParameter("@id", id) });
            if (dt.Rows.Count > 0)
            {
                txtName.Text = dt.Rows[0]["name"].ToString();
                txtPhone.Text = dt.Rows[0]["phone"].ToString();
                txtEmail.Text = dt.Rows[0]["email"].ToString();
                txtAddress.Text = dt.Rows[0]["address"].ToString();
                txtNotes.Text = dt.Rows[0]["notes"].ToString();
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text)) { MessageBox.Show("Введіть назву!"); this.DialogResult = DialogResult.None; return; }

            string query = id.HasValue
                ? "UPDATE suppliers SET name=@name, phone=@phone, email=@email, address=@addr, notes=@notes WHERE supplier_id=@id"
                : "INSERT INTO suppliers (name, phone, email, address, notes) VALUES (@name, @phone, @email, @addr, @notes)";

            var p = new[] {
                new MySqlParameter("@name", txtName.Text),
                new MySqlParameter("@phone", txtPhone.Text),
                new MySqlParameter("@email", txtEmail.Text),
                new MySqlParameter("@addr", txtAddress.Text),
                new MySqlParameter("@notes", txtNotes.Text)
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
