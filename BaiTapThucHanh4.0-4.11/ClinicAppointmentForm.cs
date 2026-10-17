using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace BaiTapThucHanh4_9_4_11
{
    public class Appointment
    {
        public string AppointmentId { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = "Chờ khám";
    }

    public class ClinicAppointmentForm : Form
    {
        private TableLayoutPanel mainLayout = null!;
        private TextBox txtId = null!, txtName = null!, txtPhone = null!, txtReason = null!, txtSearch = null!;
        private DateTimePicker dtpAppointment = null!, dtpFilterDate = null!;
        private ComboBox cboDoctor = null!, cboStatus = null!, cboFilterStatus = null!;
        private CheckBox chkFilterByDate = null!;
        private Button btnAdd = null!, btnUpdate = null!, btnDelete = null!, btnReset = null!;
        private DataGridView dgvAppointments = null!;
        private StatusStrip statusStrip = null!;
        private ToolStripStatusLabel lblCountStatus = null!;
        private MenuStrip menuStrip = null!;
        private ErrorProvider errorProvider = null!;

        private BindingList<Appointment> masterList = new BindingList<Appointment>();
        private BindingList<Appointment> displayList = new BindingList<Appointment>();
        private BindingSource bindingSource = new BindingSource();

        public ClinicAppointmentForm()
        {
            SetupForm();
            SetupControls();
            SetupBindings();
            InitSampleData();
            ApplyFilter();
        }

        private void SetupForm()
        {
            this.Text = "Clinic Appointment Manager - Quản lý lịch hẹn phòng khám";
            this.Size = new Size(1100, 720);
            this.MinimumSize = new Size(950, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9.5F);
            errorProvider = new ErrorProvider(this) { BlinkStyle = ErrorBlinkStyle.NeverBlink };
        }

        private void SetupControls()
        {
            // 1. MenuStrip
            menuStrip = new MenuStrip();
            ToolStripMenuItem mnuFile = new ToolStripMenuItem("Tệp (&F)");
            ToolStripMenuItem mnuExport = new ToolStripMenuItem("Xuất danh sách ra CSV (&E)", null, (s, e) => ExportToCsv());
            ToolStripMenuItem mnuExit = new ToolStripMenuItem("Thoát (&X)", null, (s, e) => this.Close());
            mnuFile.DropDownItems.AddRange(new ToolStripItem[] { mnuExport, new ToolStripSeparator(), mnuExit });
            menuStrip.Items.Add(mnuFile);
            this.MainMenuStrip = menuStrip;

            // 2. Main Layout (TableLayoutPanel chia form làm 2 hàng: trên nhập/lọc, dưới bảng dữ liệu)
            mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(10)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 270F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            // Top Panel (Nhập liệu và bộ lọc)
            Panel topPanel = new Panel { Dock = DockStyle.Fill };

            GroupBox grpInput = new GroupBox
            {
                Text = "Thông tin lịch hẹn",
                Location = new Point(5, 5),
                Size = new Size(680, 255),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            Label lblId = new Label { Text = "Mã hẹn:", Location = new Point(20, 28), AutoSize = true };
            txtId = new TextBox { Location = new Point(100, 25), Width = 180 };

            Label lblName = new Label { Text = "Họ tên:", Location = new Point(20, 63), AutoSize = true };
            txtName = new TextBox { Location = new Point(100, 60), Width = 230 };

            Label lblPhone = new Label { Text = "Số ĐT:", Location = new Point(360, 63), AutoSize = true };
            txtPhone = new TextBox { Location = new Point(440, 60), Width = 180 };

            Label lblDate = new Label { Text = "Ngày giờ:", Location = new Point(20, 98), AutoSize = true };
            dtpAppointment = new DateTimePicker
            {
                Location = new Point(100, 95),
                Width = 230,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd/MM/yyyy HH:mm"
            };

            Label lblDoctor = new Label { Text = "Bác sĩ:", Location = new Point(360, 98), AutoSize = true };
            cboDoctor = new ComboBox { Location = new Point(440, 95), Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
            cboDoctor.Items.AddRange(new object[] { "BS. Nguyễn Văn A", "BS. Trần Thị B", "BS. Lê Văn C", "BS. Hoàng Đức D" });
            cboDoctor.SelectedIndex = 0;

            Label lblStatus = new Label { Text = "Trạng thái:", Location = new Point(20, 133), AutoSize = true };
            cboStatus = new ComboBox { Location = new Point(100, 130), Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
            cboStatus.Items.AddRange(new object[] { "Chờ khám", "Đang khám", "Hoàn tất", "Đã hủy" });
            cboStatus.SelectedIndex = 0;

            Label lblReason = new Label { Text = "Lý do khám:", Location = new Point(20, 168), AutoSize = true };
            txtReason = new TextBox { Location = new Point(100, 165), Width = 520 };

            btnAdd = new Button { Text = "Thêm mới", Location = new Point(100, 205), Size = new Size(100, 32), BackColor = Color.FromArgb(0, 120, 215), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnUpdate = new Button { Text = "Cập nhật", Location = new Point(210, 205), Size = new Size(100, 32) };
            btnDelete = new Button { Text = "Xóa", Location = new Point(320, 205), Size = new Size(100, 32), BackColor = Color.FromArgb(217, 83, 79), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnReset = new Button { Text = "Làm mới", Location = new Point(430, 205), Size = new Size(100, 32) };

            btnAdd.Click += (s, e) => AddAppointment();
            btnUpdate.Click += (s, e) => UpdateAppointment();
            btnDelete.Click += (s, e) => DeleteAppointment();
            btnReset.Click += (s, e) => ResetInputFields();

            grpInput.Controls.AddRange(new Control[] {
                lblId, txtId, lblName, txtName, lblPhone, txtPhone,
                lblDate, dtpAppointment, lblDoctor, cboDoctor,
                lblStatus, cboStatus, lblReason, txtReason,
                btnAdd, btnUpdate, btnDelete, btnReset
            });

            // GroupBox Bộ lọc tra cứu
            GroupBox grpFilter = new GroupBox
            {
                Text = "Tra cứu và Bộ lọc",
                Location = new Point(700, 5),
                Size = new Size(365, 255),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            Label lblSearch = new Label { Text = "Tìm kiếm (Tên/SĐT):", Location = new Point(15, 28), AutoSize = true };
            txtSearch = new TextBox { Location = new Point(15, 50), Width = 330 };
            txtSearch.TextChanged += (s, e) => ApplyFilter();

            chkFilterByDate = new CheckBox { Text = "Lọc theo ngày hẹn:", Location = new Point(15, 85), AutoSize = true };
            chkFilterByDate.CheckedChanged += (s, e) => ApplyFilter();

            dtpFilterDate = new DateTimePicker { Location = new Point(15, 110), Width = 330, Format = DateTimePickerFormat.Short };
            dtpFilterDate.ValueChanged += (s, e) => { if (chkFilterByDate.Checked) ApplyFilter(); };

            Label lblFStatus = new Label { Text = "Lọc theo trạng thái:", Location = new Point(15, 145), AutoSize = true };
            cboFilterStatus = new ComboBox { Location = new Point(15, 170), Width = 330, DropDownStyle = ComboBoxStyle.DropDownList };
            cboFilterStatus.Items.AddRange(new object[] { "Tất cả", "Chờ khám", "Đang khám", "Hoàn tất", "Đã hủy" });
            cboFilterStatus.SelectedIndex = 0;
            cboFilterStatus.SelectedIndexChanged += (s, e) => ApplyFilter();

            grpFilter.Controls.AddRange(new Control[] { lblSearch, txtSearch, chkFilterByDate, dtpFilterDate, lblFStatus, cboFilterStatus });

            topPanel.Controls.Add(grpInput);
            topPanel.Controls.Add(grpFilter);

            // 3. DataGridView
            dgvAppointments = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White
            };

            dgvAppointments.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { DataPropertyName = "AppointmentId", HeaderText = "Mã hẹn", Width = 90 },
                new DataGridViewTextBoxColumn { DataPropertyName = "PatientName", HeaderText = "Họ tên bệnh nhân", Width = 180 },
                new DataGridViewTextBoxColumn { DataPropertyName = "PhoneNumber", HeaderText = "Số điện thoại", Width = 110 },
                new DataGridViewTextBoxColumn { DataPropertyName = "AppointmentDate", HeaderText = "Ngày giờ hẹn", Width = 140, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy HH:mm" } },
                new DataGridViewTextBoxColumn { DataPropertyName = "DoctorName", HeaderText = "Bác sĩ phụ trách", Width = 160 },
                new DataGridViewTextBoxColumn { DataPropertyName = "Reason", HeaderText = "Lý do khám", Width = 230 },
                new DataGridViewTextBoxColumn { DataPropertyName = "Status", HeaderText = "Trạng thái", Width = 110 }
            });

            dgvAppointments.SelectionChanged += DgvAppointments_SelectionChanged;

            mainLayout.Controls.Add(topPanel, 0, 0);
            mainLayout.Controls.Add(dgvAppointments, 0, 1);

            // 4. StatusStrip
            statusStrip = new StatusStrip();
            lblCountStatus = new ToolStripStatusLabel("Tổng số lịch hẹn: 0");
            statusStrip.Items.Add(lblCountStatus);

            this.Controls.Add(mainLayout);
            this.Controls.Add(statusStrip);
            this.Controls.Add(menuStrip);
        }

        private void SetupBindings()
        {
            bindingSource.DataSource = displayList;
            dgvAppointments.DataSource = bindingSource;
        }

        private void InitSampleData()
        {
            DateTime now = DateTime.Now;
            masterList.Add(new Appointment { AppointmentId = "LH001", PatientName = "Trần Minh Tâm", PhoneNumber = "0912345678", AppointmentDate = now.AddHours(2), DoctorName = "BS. Nguyễn Văn A", Reason = "Đau đầu, sốt nhẹ", Status = "Chờ khám" });
            masterList.Add(new Appointment { AppointmentId = "LH002", PatientName = "Lê Hoàng Yến", PhoneNumber = "0987654321", AppointmentDate = now.AddHours(4), DoctorName = "BS. Trần Thị B", Reason = "Tái khám huyết áp", Status = "Đang khám" });
            masterList.Add(new Appointment { AppointmentId = "LH003", PatientName = "Phạm Gia Bảo", PhoneNumber = "0905123456", AppointmentDate = now.AddDays(1), DoctorName = "BS. Lê Văn C", Reason = "Khám răng định kỳ", Status = "Chờ khám" });
        }

        private void DgvAppointments_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvAppointments.CurrentRow?.DataBoundItem is Appointment selected)
            {
                txtId.Text = selected.AppointmentId;
                txtId.ReadOnly = true; // Không sửa mã khi chọn dòng
                txtName.Text = selected.PatientName;
                txtPhone.Text = selected.PhoneNumber;
                dtpAppointment.Value = selected.AppointmentDate;
                cboDoctor.SelectedItem = selected.DoctorName;
                cboStatus.SelectedItem = selected.Status;
                txtReason.Text = selected.Reason;
                errorProvider.Clear();
            }
        }

        private bool ValidateForm(bool isNew)
        {
            errorProvider.Clear();
            bool isValid = true;

            string id = txtId.Text.Trim();
            if (string.IsNullOrWhiteSpace(id))
            {
                errorProvider.SetError(txtId, "Mã lịch hẹn không được để trống!");
                isValid = false;
            }
            else if (isNew && masterList.Any(a => a.AppointmentId.Equals(id, StringComparison.OrdinalIgnoreCase)))
            {
                errorProvider.SetError(txtId, "Mã lịch hẹn đã tồn tại!");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                errorProvider.SetError(txtName, "Họ tên không được để trống!");
                isValid = false;
            }

            string phone = txtPhone.Text.Trim();
            if (string.IsNullOrWhiteSpace(phone) || !Regex.IsMatch(phone, @"^(0[3|5|7|8|9])+([0-9]{8})$"))
            {
                errorProvider.SetError(txtPhone, "Số điện thoại không hợp lệ (cần 10 chữ số, bắt đầu bằng 03, 05, 07, 08, 09)!");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(txtReason.Text))
            {
                errorProvider.SetError(txtReason, "Lý do khám không được để trống!");
                isValid = false;
            }

            if (isNew && dtpAppointment.Value < DateTime.Now.AddMinutes(-5))
            {
                errorProvider.SetError(dtpAppointment, "Ngày giờ hẹn không được nằm trong quá khứ!");
                isValid = false;
            }

            return isValid;
        }

        private void AddAppointment()
        {
            if (!ValidateForm(isNew: true)) return;

            var newApp = new Appointment
            {
                AppointmentId = txtId.Text.Trim(),
                PatientName = txtName.Text.Trim(),
                PhoneNumber = txtPhone.Text.Trim(),
                AppointmentDate = dtpAppointment.Value,
                DoctorName = cboDoctor.SelectedItem?.ToString() ?? "",
                Status = cboStatus.SelectedItem?.ToString() ?? "Chờ khám",
                Reason = txtReason.Text.Trim()
            };

            masterList.Add(newApp);
            ApplyFilter();
            MessageBox.Show("Thêm lịch hẹn mới thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ResetInputFields();
        }

        private void UpdateAppointment()
        {
            string id = txtId.Text.Trim();
            var existing = masterList.FirstOrDefault(a => a.AppointmentId.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (existing == null)
            {
                MessageBox.Show("Vui lòng chọn một lịch hẹn từ danh sách để cập nhật!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ValidateForm(isNew: false)) return;

            existing.PatientName = txtName.Text.Trim();
            existing.PhoneNumber = txtPhone.Text.Trim();
            existing.AppointmentDate = dtpAppointment.Value;
            existing.DoctorName = cboDoctor.SelectedItem?.ToString() ?? "";
            existing.Status = cboStatus.SelectedItem?.ToString() ?? "Chờ khám";
            existing.Reason = txtReason.Text.Trim();

            ApplyFilter();
            MessageBox.Show("Cập nhật thông tin lịch hẹn thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void DeleteAppointment()
        {
            string id = txtId.Text.Trim();
            var existing = masterList.FirstOrDefault(a => a.AppointmentId.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (existing == null)
            {
                MessageBox.Show("Vui lòng chọn lịch hẹn cần xóa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var dr = MessageBox.Show($"Xác nhận xóa lịch hẹn [{existing.AppointmentId}] của bệnh nhân [{existing.PatientName}]?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr == DialogResult.Yes)
            {
                masterList.Remove(existing);
                ApplyFilter();
                ResetInputFields();
                MessageBox.Show("Đã xóa lịch hẹn thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ResetInputFields()
        {
            txtId.ReadOnly = false;
            txtId.Clear();
            txtName.Clear();
            txtPhone.Clear();
            dtpAppointment.Value = DateTime.Now.AddHours(1);
            cboDoctor.SelectedIndex = 0;
            cboStatus.SelectedIndex = 0;
            txtReason.Clear();
            errorProvider.Clear();
            txtId.Focus();
        }

        private void ApplyFilter()
        {
            string keyword = txtSearch.Text.Trim().ToLower();
            string statusFilter = cboFilterStatus.SelectedItem?.ToString() ?? "Tất cả";
            bool filterDate = chkFilterByDate.Checked;
            DateTime targetDate = dtpFilterDate.Value.Date;

            var filtered = masterList.Where(a =>
                (string.IsNullOrEmpty(keyword) || a.PatientName.ToLower().Contains(keyword) || a.PhoneNumber.Contains(keyword)) &&
                (statusFilter == "Tất cả" || a.Status == statusFilter) &&
                (!filterDate || a.AppointmentDate.Date == targetDate)
            ).ToList();

            displayList.Clear();
            foreach (var item in filtered) displayList.Add(item);

            lblCountStatus.Text = $"Số lịch hẹn hiển thị: {displayList.Count} / Tổng số: {masterList.Count}";
        }

        private void ExportToCsv()
        {
            using SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "CSV File (*.csv)|*.csv",
                FileName = $"DanhSachLichHen_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("Mã hẹn,Họ tên bệnh nhân,Số điện thoại,Ngày giờ hẹn,Bác sĩ,Trạng thái,Lý do");
                    foreach (var a in displayList)
                    {
                        sb.AppendLine($"\"{a.AppointmentId}\",\"{a.PatientName}\",\"{a.PhoneNumber}\",\"{a.AppointmentDate:dd/MM/yyyy HH:mm}\",\"{a.DoctorName}\",\"{a.Status}\",\"{a.Reason}\"");
                    }
                    File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("Xuất file CSV thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi ghi file: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}