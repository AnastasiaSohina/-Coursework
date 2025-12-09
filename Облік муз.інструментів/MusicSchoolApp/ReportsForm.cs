using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace MusicSchoolApp
{
    public partial class ReportsForm : Form
    {
        private DataGridView dgv;
        private ComboBox cmbReportType;
        private Button btnGenerate;

        public ReportsForm()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Size = new Size(1000, 600);

            Label lblTitle = new Label
            {
                Text = "Звіти",
                Font = new Font("Arial", 16, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };

            Label lblType = new Label { Text = "Тип звіту:", Location = new Point(20, 60), AutoSize = true };
            cmbReportType = new ComboBox { Location = new Point(120, 58), Size = new Size(300, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbReportType.Items.AddRange(new[] {
                "Список всіх учнів",
                "Учні по класах",
                "Інструменти в користуванні",
                "Історія ремонтів",
                "Розподіл по предметах"
            });
            cmbReportType.SelectedIndex = 0;

            btnGenerate = new Button
            {
                Text = "Сформувати звіт",
                Location = new Point(440, 58),
                Size = new Size(140, 30),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White
            };
            btnGenerate.Click += BtnGenerate_Click;

            dgv = new DataGridView
            {
                Location = new Point(20, 100),
                Size = new Size(950, 450),
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            this.Controls.AddRange(new Control[] { lblTitle, lblType, cmbReportType, btnGenerate, dgv });
        }

        private void BtnGenerate_Click(object sender, EventArgs e)
        {
            string query = "";
            switch (cmbReportType.SelectedIndex)
            {
                case 0:
                    query = "SELECT student_id, CONCAT(last_name, ' ', first_name) AS 'ПІБ', class AS 'Клас', phone AS 'Телефон', email AS 'Email' FROM students ORDER BY last_name";
                    break;
                case 1:
                    query = "SELECT class AS 'Клас', COUNT(*) AS 'Кількість учнів' FROM students GROUP BY class ORDER BY class";
                    break;
                case 2:
                    query = @"SELECT CONCAT(s.last_name, ' ', s.first_name) AS 'Учень', i.name AS 'Інструмент',
                        ia.issue_date AS 'Дата видачі' FROM instrument_assignments ia
                        JOIN students s ON ia.student_id = s.student_id
                        JOIN instruments i ON ia.instrument_id = i.instrument_id
                        WHERE ia.return_date IS NULL ORDER BY ia.issue_date DESC";
                    break;
                case 3:
                    query = @"SELECT i.name AS 'Інструмент', ir.repair_date AS 'Дата', ir.cost AS 'Вартість', ir.description AS 'Опис'
                        FROM instrument_repairs ir JOIN instruments i ON ir.instrument_id = i.instrument_id ORDER BY ir.repair_date DESC";
                    break;
                case 4:
                    query = @"SELECT subj.name AS 'Предмет', COUNT(ss.id) AS 'Кількість учнів'
                        FROM subjects subj LEFT JOIN student_subjects ss ON subj.subject_id = ss.subject_id
                        GROUP BY subj.subject_id ORDER BY COUNT(ss.id) DESC";
                    break;
            }

            dgv.DataSource = DatabaseHelper.ExecuteQuery(query);
        }
    }
}