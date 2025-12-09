using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace MusicSchoolApp
{
    public partial class InstrumentsForm : Form
    {
        private DataGridView dgvInstruments;
        private TextBox txtSearch;
        private ComboBox cmbCategory, cmbCondition;
        private Button btnAdd, btnEdit, btnDelete, btnRefresh;
        private Label lblAccessInfo;

        public InstrumentsForm()
        {
            InitializeComponents();
            ApplyPermissions();
            LoadInstruments();
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
            this.Size = new Size(950, 550);

            Label lblTitle = new Label
            {
                Text = "Управління інструментами",
                Font = new Font("Arial", 16, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };

            lblAccessInfo = new Label
            {
                Text = "ℹ️ Staff: Ви можете редагувати інструменти. Створення/видалення - тільки для адміністраторів.",
                ForeColor = Color.DarkBlue,
                Location = new Point(20, 50),
                Size = new Size(900, 20),
                Visible = false
            };

            Label lblSearch = new Label
            {
                Text = "Пошук:",
                Location = new Point(20, 85),
                AutoSize = true
            };
            txtSearch = new TextBox
            {
                Location = new Point(80, 82),
                Size = new Size(200, 25)
            };
            txtSearch.TextChanged += (s, e) => LoadInstruments();

            Label lblCategory = new Label
            {
                Text = "Категорія:",
                Location = new Point(300, 85),
                AutoSize = true
            };
            cmbCategory = new ComboBox
            {
                Location = new Point(380, 82),
                Size = new Size(150, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbCategory.Items.AddRange(new[] { "Всі", "Струнні", "Духові", "Клавішні", "Ударні" });
            cmbCategory.SelectedIndex = 0;
            cmbCategory.SelectedIndexChanged += (s, e) => LoadInstruments();

            Label lblCondition = new Label
            {
                Text = "Стан:",
                Location = new Point(550, 85),
                AutoSize = true
            };
            cmbCondition = new ComboBox
            {
                Location = new Point(600, 82),
                Size = new Size(120, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbCondition.Items.AddRange(new[] { "Всі", "Відмінний", "Добрий", "Задовільний", "Потребує ремонту" });
            cmbCondition.SelectedIndex = 0;
            cmbCondition.SelectedIndexChanged += (s, e) => LoadInstruments();

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
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(244, 67, 54),
                ForeColor = Color.White
            };
            btnDelete.Click += BtnDelete_Click;

            btnRefresh = new Button
            {
                Text = "Оновити",
                Location = new Point(350, 120),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(158, 158, 158),
                ForeColor = Color.White
            };
            btnRefresh.Click += (s, e) => LoadInstruments();

            dgvInstruments = new DataGridView
            {
                Location = new Point(20, 160),
                Size = new Size(900, 340),
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            this.Controls.AddRange(new Control[]
            {
                lblTitle, lblAccessInfo,
                lblSearch, txtSearch,
                lblCategory, cmbCategory,
                lblCondition, cmbCondition,
                btnAdd, btnEdit, btnDelete, btnRefresh,
                dgvInstruments
            });
        }

        private Button CreateButton(string text, Point location, Color backColor)
        {
            return new Button { Text = text, Location = location, Size = new Size(100, 30), BackColor = backColor, ForeColor = Color.White };
        }

        private void LoadInstruments()
        {
            string query = @"SELECT instrument_id AS 'ID', name AS 'Назва', category AS 'Категорія',
                                   brand AS 'Бренд', model AS 'Модель', condition_status AS 'Стан',
                                   location AS 'Розташування', price AS 'Ціна'
                            FROM instruments WHERE 1=1";

            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
                query += $" AND (name LIKE '%{txtSearch.Text}%' OR brand LIKE '%{txtSearch.Text}%' OR model LIKE '%{txtSearch.Text}%')";

            if (cmbCategory.SelectedIndex > 0)
                query += $" AND category = '{cmbCategory.SelectedItem}'";

            if (cmbCondition.SelectedIndex > 0)
                query += $" AND condition_status = '{cmbCondition.SelectedItem}'";

            query += " ORDER BY name";
            dgvInstruments.DataSource = DatabaseHelper.ExecuteQuery(query);
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (!UserSession.CanAddInstruments)
            {
                MessageBox.Show(
                    "Тільки адміністратор може додавати нові інструменти!",
                    "Доступ заборонено",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            InstrumentEditForm form = new InstrumentEditForm();
            if (form.ShowDialog() == DialogResult.OK) LoadInstruments();
        }


        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgvInstruments.SelectedRows.Count == 0)
            {
                MessageBox.Show("Оберіть інструмент!", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            int id = Convert.ToInt32(dgvInstruments.SelectedRows[0].Cells["ID"].Value);
            InstrumentEditForm form = new InstrumentEditForm(id);
            if (form.ShowDialog() == DialogResult.OK) LoadInstruments();
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (!UserSession.CanDeleteInstruments)
            {
                MessageBox.Show(
                    "Тільки адміністратор може видаляти інструменти!",
                    "Доступ заборонено",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            if (dgvInstruments.SelectedRows.Count == 0)
            {
                MessageBox.Show("Оберіть інструмент!", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Видалити інструмент?", "Підтвердження",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                int id = Convert.ToInt32(dgvInstruments.SelectedRows[0].Cells["ID"].Value);
                DatabaseHelper.ExecuteNonQuery(
                    "DELETE FROM instruments WHERE instrument_id = @id",
                    new[] { new MySqlParameter("@id", id) }
                );
                LoadInstruments();
            }
        }

    }

    public partial class InstrumentEditForm : Form
    {
        private int? instrumentId;
        private TextBox txtName, txtBrand, txtModel, txtSerial, txtLocation, txtNotes;
        private ComboBox cmbCategory, cmbCondition;
        private DateTimePicker dtpPurchase;
        private NumericUpDown nudPrice;
        private Button btnSave, btnCancel;

        public InstrumentEditForm(int? id = null)
        {
            instrumentId = id;
            InitializeComponent();
            if (instrumentId.HasValue) LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = instrumentId.HasValue ? "Редагування інструменту" : "Додавання інструменту";
            this.Size = new Size(500, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            int y = 20;
            AddField("Назва:", ref txtName, ref y);

            Label lblCategory = new Label { Text = "Категорія:", Location = new Point(20, y), AutoSize = true };
            cmbCategory = new ComboBox { Location = new Point(150, y), Size = new Size(300, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbCategory.Items.AddRange(new[] { "Струнні", "Духові", "Клавішні", "Ударні" });
            cmbCategory.SelectedIndex = 0;
            this.Controls.AddRange(new Control[] { lblCategory, cmbCategory });
            y += 40;

            AddField("Бренд:", ref txtBrand, ref y);
            AddField("Модель:", ref txtModel, ref y);
            AddField("Серійний номер:", ref txtSerial, ref y);

            Label lblPurchase = new Label { Text = "Дата закупівлі:", Location = new Point(20, y), AutoSize = true };
            dtpPurchase = new DateTimePicker { Location = new Point(150, y), Size = new Size(300, 25), Format = DateTimePickerFormat.Short };
            this.Controls.AddRange(new Control[] { lblPurchase, dtpPurchase });
            y += 40;

            Label lblCondition = new Label { Text = "Стан:", Location = new Point(20, y), AutoSize = true };
            cmbCondition = new ComboBox { Location = new Point(150, y), Size = new Size(300, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbCondition.Items.AddRange(new[] { "Відмінний", "Добрий", "Задовільний", "Потребує ремонту" });
            cmbCondition.SelectedIndex = 0;
            this.Controls.AddRange(new Control[] { lblCondition, cmbCondition });
            y += 40;

            Label lblPrice = new Label { Text = "Ціна:", Location = new Point(20, y), AutoSize = true };
            nudPrice = new NumericUpDown { Location = new Point(150, y), Size = new Size(300, 25), Maximum = 1000000, DecimalPlaces = 2 };
            this.Controls.AddRange(new Control[] { lblPrice, nudPrice });
            y += 40;

            AddField("Розташування:", ref txtLocation, ref y);

            Label lblNotes = new Label { Text = "Примітки:", Location = new Point(20, y), AutoSize = true };
            txtNotes = new TextBox { Location = new Point(150, y), Size = new Size(300, 60), Multiline = true };
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

            btnCancel = new Button { Text = "Скасувати", Location = new Point(280, y), Size = new Size(120, 35), DialogResult = DialogResult.Cancel };
            this.Controls.AddRange(new Control[] { btnSave, btnCancel });
        }

        private void AddField(string label, ref TextBox textBox, ref int y)
        {
            Label lbl = new Label { Text = label, Location = new Point(20, y), AutoSize = true };
            textBox = new TextBox { Location = new Point(150, y), Size = new Size(300, 25) };
            this.Controls.AddRange(new Control[] { lbl, textBox });
            y += 40;
        }

        private void LoadData()
        {
            DataTable dt = DatabaseHelper.ExecuteQuery("SELECT * FROM instruments WHERE instrument_id = @id",
                new[] { new MySqlParameter("@id", instrumentId) });
            if (dt.Rows.Count > 0)
            {
                DataRow r = dt.Rows[0];
                txtName.Text = r["name"].ToString();
                cmbCategory.SelectedItem = r["category"].ToString();
                txtBrand.Text = r["brand"].ToString();
                txtModel.Text = r["model"].ToString();
                txtSerial.Text = r["serial_number"].ToString();
                if (r["purchase_date"] != DBNull.Value) dtpPurchase.Value = Convert.ToDateTime(r["purchase_date"]);
                cmbCondition.SelectedItem = r["condition_status"].ToString();
                if (r["price"] != DBNull.Value) nudPrice.Value = Convert.ToDecimal(r["price"]);
                txtLocation.Text = r["location"].ToString();
                txtNotes.Text = r["notes"].ToString();
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введіть назву інструменту!", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.None;
                return;
            }

            string query = instrumentId.HasValue
                ? @"UPDATE instruments SET name=@name, category=@cat, brand=@brand, model=@model, serial_number=@serial,
                   purchase_date=@pdate, condition_status=@cond, price=@price, location=@loc, notes=@notes WHERE instrument_id=@id"
                : @"INSERT INTO instruments (name, category, brand, model, serial_number, purchase_date, condition_status, price, location, notes)
                   VALUES (@name, @cat, @brand, @model, @serial, @pdate, @cond, @price, @loc, @notes)";

            var p = new[] {
                new MySqlParameter("@name", txtName.Text),
                new MySqlParameter("@cat", cmbCategory.SelectedItem),
                new MySqlParameter("@brand", txtBrand.Text),
                new MySqlParameter("@model", txtModel.Text),
                new MySqlParameter("@serial", txtSerial.Text),
                new MySqlParameter("@pdate", dtpPurchase.Value.Date),
                new MySqlParameter("@cond", cmbCondition.SelectedItem),
                new MySqlParameter("@price", nudPrice.Value),
                new MySqlParameter("@loc", txtLocation.Text),
                new MySqlParameter("@notes", txtNotes.Text)
            };

            if (instrumentId.HasValue)
            {
                Array.Resize(ref p, p.Length + 1);
                p[p.Length - 1] = new MySqlParameter("@id", instrumentId);
            }

            if (DatabaseHelper.ExecuteNonQuery(query, p) > 0)
                MessageBox.Show("Збережено!", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                this.DialogResult = DialogResult.None;
        }
    }
}