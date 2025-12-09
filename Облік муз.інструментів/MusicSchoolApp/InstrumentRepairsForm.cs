using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace MusicSchoolApp
{
    public partial class InstrumentRepairsForm : Form
    {
        private DataGridView dgv;
        private Button btnAdd, btnEdit, btnDelete;

        public InstrumentRepairsForm()
        {
            InitializeComponents();
            LoadData();
        }

        private void InitializeComponents()
        {
            this.Size = new Size(1000, 600);

            Label lblTitle = new Label
            {
                Text = "Ремонт інструментів",
                Font = new Font("Arial", 16, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };

            btnAdd = new Button
            {
                Text = "Додати",
                Location = new Point(20, 60),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White
            };
            btnAdd.Click += (s, e) => { RepairEditForm f = new RepairEditForm(); if (f.ShowDialog() == DialogResult.OK) LoadData(); };

            btnEdit = new Button
            {
                Text = "Редагувати",
                Location = new Point(130, 60),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(33, 150, 243),
                ForeColor = Color.White
            };
            btnEdit.Click += BtnEdit_Click;

            btnDelete = new Button
            {
                Text = "Видалити",
                Location = new Point(240, 60),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(244, 67, 54),
                ForeColor = Color.White
            };
            btnDelete.Click += BtnDelete_Click;

            dgv = new DataGridView
            {
                Location = new Point(20, 100),
                Size = new Size(950, 450),
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            this.Controls.AddRange(new Control[] { lblTitle, btnAdd, btnEdit, btnDelete, dgv });
        }

        private void LoadData()
        {
            string query = @"SELECT ir.repair_id AS 'ID', i.name AS 'Інструмент',
                ir.repair_date AS 'Дата ремонту', ir.description AS 'Опис',
                ir.cost AS 'Вартість', ir.repair_company AS 'Компанія',
                ir.responsible_person AS 'Відповідальна особа'
            FROM instrument_repairs ir
            JOIN instruments i ON ir.instrument_id = i.instrument_id
            ORDER BY ir.repair_date DESC";
            dgv.DataSource = DatabaseHelper.ExecuteQuery(query);
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0) { MessageBox.Show("Оберіть запис!"); return; }
            RepairEditForm f = new RepairEditForm(Convert.ToInt32(dgv.SelectedRows[0].Cells["ID"].Value));
            if (f.ShowDialog() == DialogResult.OK) LoadData();
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0) { MessageBox.Show("Оберіть запис!"); return; }
            if (MessageBox.Show("Видалити запис?", "Підтвердження", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                DatabaseHelper.ExecuteNonQuery("DELETE FROM instrument_repairs WHERE repair_id = @id",
                    new[] { new MySqlParameter("@id", dgv.SelectedRows[0].Cells["ID"].Value) });
                LoadData();
            }
        }
    }

    public partial class RepairEditForm : Form
    {
        private int? id;
        private ComboBox cmbInstrument;
        private DateTimePicker dtpRepair;
        private TextBox txtDesc, txtCompany, txtResponsible;
        private NumericUpDown nudCost;

        public RepairEditForm(int? repairId = null)
        {
            id = repairId;
            InitializeComponent();
            LoadComboBox();
            if (id.HasValue) LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = id.HasValue ? "Редагування ремонту" : "Додавання ремонту";
            this.Size = new Size(450, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            int y = 20;
            Label lblInst = new Label { Text = "Інструмент:", Location = new Point(20, y), AutoSize = true };
            cmbInstrument = new ComboBox { Location = new Point(150, y), Size = new Size(270, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            this.Controls.AddRange(new Control[] { lblInst, cmbInstrument });
            y += 40;

            Label lblDate = new Label { Text = "Дата ремонту:", Location = new Point(20, y), AutoSize = true };
            dtpRepair = new DateTimePicker { Location = new Point(150, y), Size = new Size(270, 25), Format = DateTimePickerFormat.Short };
            this.Controls.AddRange(new Control[] { lblDate, dtpRepair });
            y += 40;

            Label lblDesc = new Label { Text = "Опис:", Location = new Point(20, y), AutoSize = true };
            txtDesc = new TextBox { Location = new Point(150, y), Size = new Size(270, 60), Multiline = true };
            this.Controls.AddRange(new Control[] { lblDesc, txtDesc });
            y += 75;

            Label lblCost = new Label { Text = "Вартість:", Location = new Point(20, y), AutoSize = true };
            nudCost = new NumericUpDown { Location = new Point(150, y), Size = new Size(270, 25), Maximum = 1000000, DecimalPlaces = 2 };
            this.Controls.AddRange(new Control[] { lblCost, nudCost });
            y += 40;

            Label lblComp = new Label { Text = "Компанія:", Location = new Point(20, y), AutoSize = true };
            txtCompany = new TextBox { Location = new Point(150, y), Size = new Size(270, 25) };
            this.Controls.AddRange(new Control[] { lblComp, txtCompany });
            y += 40;

            Label lblResp = new Label { Text = "Відповідальний:", Location = new Point(20, y), AutoSize = true };
            txtResponsible = new TextBox { Location = new Point(150, y), Size = new Size(270, 25) };
            this.Controls.AddRange(new Control[] { lblResp, txtResponsible });
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

        private void LoadComboBox()
        {
            cmbInstrument.DataSource = DatabaseHelper.ExecuteQuery("SELECT instrument_id, CONCAT(name, ' - ', brand) AS name FROM instruments ORDER BY name");
            cmbInstrument.DisplayMember = "name";
            cmbInstrument.ValueMember = "instrument_id";
        }

        private void LoadData()
        {
            DataTable dt = DatabaseHelper.ExecuteQuery("SELECT * FROM instrument_repairs WHERE repair_id = @id", new[] { new MySqlParameter("@id", id) });
            if (dt.Rows.Count > 0)
            {
                cmbInstrument.SelectedValue = dt.Rows[0]["instrument_id"];
                dtpRepair.Value = Convert.ToDateTime(dt.Rows[0]["repair_date"]);
                txtDesc.Text = dt.Rows[0]["description"].ToString();
                if (dt.Rows[0]["cost"] != DBNull.Value) nudCost.Value = Convert.ToDecimal(dt.Rows[0]["cost"]);
                txtCompany.Text = dt.Rows[0]["repair_company"].ToString();
                txtResponsible.Text = dt.Rows[0]["responsible_person"].ToString();
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            string query = id.HasValue
                ? "UPDATE instrument_repairs SET instrument_id=@iid, repair_date=@date, description=@desc, cost=@cost, repair_company=@comp, responsible_person=@resp WHERE repair_id=@id"
                : "INSERT INTO instrument_repairs (instrument_id, repair_date, description, cost, repair_company, responsible_person) VALUES (@iid, @date, @desc, @cost, @comp, @resp)";

            var p = new[] {
                new MySqlParameter("@iid", cmbInstrument.SelectedValue),
                new MySqlParameter("@date", dtpRepair.Value.Date),
                new MySqlParameter("@desc", txtDesc.Text),
                new MySqlParameter("@cost", nudCost.Value),
                new MySqlParameter("@comp", txtCompany.Text),
                new MySqlParameter("@resp", txtResponsible.Text)
            };

            if (id.HasValue)
            {
                Array.Resize(ref p, 7);
                p[6] = new MySqlParameter("@id", id);
            }

            DatabaseHelper.ExecuteNonQuery(query, p);
            MessageBox.Show("Збережено!");
        }
    }
}