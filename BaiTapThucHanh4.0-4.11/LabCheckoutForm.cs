using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BaiTapThucHanh4_9_4_11
{
    public class CheckoutTicket
    {
        public string TicketId { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string EquipmentName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string Status { get; set; } = "Đang mượn";
    }

    public class LabCheckoutForm : Form
    {
        private TextBox txtTicketId = null!, txtStudentId = null!, txtStudentName = null!, txtQuantity = null!, txtSearch = null!;
        private ComboBox cboEquipment = null!, cboStatus = null!, cboFilterStatus = null!;
        private DateTimePicker dtpBorrowDate = null!, dtpDueDate = null!, dtpReturnDate = null!;
        private CheckBox chkReturned = null!;
        private Button btnAdd = null!, btnUpdate = null!, btnDelete = null!, btnMarkReturned = null!, btnReset = null!;
        private DataGridView dgvTickets = null!;
        private StatusStrip statusStrip = null!;
        private ToolStripStatusLabel lblSummaryStatus = null!;
        private MenuStrip menuStrip = null!;
        private ErrorProvider errorProvider = null!;

        private BindingList<CheckoutTicket> masterList = new BindingList<CheckoutTicket>();
        private BindingList<CheckoutTicket> displayList = new BindingList<CheckoutTicket>();
        private BindingSource bindingSource = new BindingSource();

        public LabCheckoutForm()
        {
            SetupForm();
            SetupControls();
            SetupBindings();
            InitSampleData();
            UpdateAllTicketStatuses();
            ApplyFilter();
        }

        private void SetupForm()
        {
            this.Text = "Lab Equipment Checkout - Quản lý mượn trả thiết bị thực hành";
            this.Size = new Size(1150, 750);
            this.MinimumSize = new Size(1000, 620);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9.5F);
            errorProvider = new ErrorProvider(this) { BlinkStyle = ErrorBlinkStyle.NeverBlink };
        }

        private void SetupControls()
        {
            // Menu
            menuStrip = new MenuStrip();
            ToolStripMenuItem mnuFile = new ToolStripMenuItem("Tệp (&F)");
            ToolStripMenuItem mnuExport = new ToolStripMenuItem("Xuất danh sách ra CSV (&E)", null, (s, e) => ExportToCsv());
            ToolStripMenuItem mnuExit = new ToolStripMenuItem("Thoát (&X)", null, (s, e) => this.Close());
            mnuFile.DropDownItems.AddRange(new ToolStripItem[] { mnuExport, new ToolStripSeparator(), mnuExit });
            menuStrip.Items.Add(mnuFile);
            this.MainMenuStrip = menuStrip;

            // Panel nhập liệu
            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 280, Padding = new Padding(10) };

            GroupBox grpInput = new GroupBox
            {
                Text = "Thông tin phiếu mượn thiết bị",
                Location = new Point(10, 5),
                Size = new Size(720, 265),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            Label lblTId = new Label { Text = "Mã phiếu:", Location = new Point(20, 28), AutoSize = true };
            txtTicketId = new TextBox { Location = new Point(100, 25), Width = 150 };

            Label lblSId = new Label { Text = "Mã SV:", Location = new Point(280, 28), AutoSize = true };
            txtStudentId = new TextBox { Location = new Point(340, 25), Width = 140 };

            Label lblSName = new Label { Text = "Họ tên SV:", Location = new Point(20, 65), AutoSize = true };
            txtStudentName = new TextBox { Location = new Point(100, 62), Width = 200 };

            Label lblEquip = new Label { Text = "Thiết bị:", Location = new Point(320, 65), AutoSize = true };
            cboEquipment = new ComboBox { Location = new Point(390, 62), Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
            cboEquipment.Items.AddRange(new object[] { "Bộ định tuyến Cisco 2911", "Switch Cisco 2960", "Kit Arduino Uno R3", "Kit Raspberry Pi 4", "Máy hiện sóng Digital Oscilloscope", "Đồng hồ vạn năng Fluke" });
            cboEquipment.SelectedIndex = 0;

            Label lblQty = new Label { Text = "Số lượng:", Location = new Point(590, 65), AutoSize = true };
            txtQuantity = new TextBox { Location = new Point(650, 62), Width = 50, Text = "1" };

            Label lblBDate = new Label { Text = "Ngày mượn:", Location = new Point(20, 105), AutoSize = true };
            dtpBorrowDate = new DateTimePicker { Location = new Point(100, 102), Width = 130, Format = DateTimePickerFormat.Short };

            Label lblDDate = new Label { Text = "Hạn trả:", Location = new Point(260, 105), AutoSize = true };
            dtpDueDate = new DateTimePicker { Location = new Point(330, 102), Width = 130, Format = DateTimePickerFormat.Short };

            chkReturned = new CheckBox { Text = "Đã trả thực tế:", Location = new Point(480, 104), AutoSize = true };
            dtpReturnDate = new DateTimePicker { Location = new Point(590, 102), Width = 110, Format = DateTimePickerFormat.Short, Enabled = false };
            chkReturned.CheckedChanged += (s, e) => dtpReturnDate.Enabled = chkReturned.Checked;

            Label lblStat = new Label { Text = "Tình trạng:", Location = new Point(20, 145), AutoSize = true };
            cboStatus = new ComboBox { Location = new Point(100, 142), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList, Enabled = false };
            cboStatus.Items.AddRange(new object[] { "Đang mượn", "Đã trả", "Quá hạn" });
            cboStatus.SelectedIndex = 0;

            btnAdd = new Button { Text = "Lập phiếu", Location = new Point(100, 195), Size = new Size(100, 32), BackColor = Color.FromArgb(0, 120, 215), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnUpdate = new Button { Text = "Cập nhật", Location = new Point(210, 195), Size = new Size(100, 32) };
            btnDelete = new Button { Text = "Xóa phiếu", Location = new Point(320, 195), Size = new Size(100, 32), BackColor = Color.FromArgb(217, 83, 79), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnMarkReturned = new Button { Text = "Đánh dấu Đã trả", Location = new Point(430, 195), Size = new Size(130, 32), BackColor = Color.SeaGreen, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnReset = new Button { Text = "Làm mới", Location = new Point(570, 195), Size = new Size(90, 32) };

            btnAdd.Click += (s, e) => AddTicket();
            btnUpdate.Click += (s, e) => UpdateTicket();
            btnDelete.Click += (s, e) => DeleteTicket();
            btnMarkReturned.Click += (s, e) => MarkAsReturned();
            btnReset.Click += (s, e) => ResetFields();

            grpInput.Controls.AddRange(new Control[] {
                lblTId, txtTicketId, lblSId, txtStudentId, lblSName, txtStudentName,
                lblEquip, cboEquipment, lblQty, txtQuantity,
                lblBDate, dtpBorrowDate, lblDDate, dtpDueDate, chkReturned, dtpReturnDate,
                lblStat, cboStatus, btnAdd, btnUpdate, btnDelete, btnMarkReturned, btnReset
            });

            GroupBox grpFilter = new GroupBox
            {
                Text = "Tìm kiếm & Phân loại",
                Location = new Point(740, 5),
                Size = new Size(385, 265),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            Label lblSearch = new Label { Text = "Tìm theo Mã SV hoặc Tên thiết bị:", Location = new Point(15, 30), AutoSize = true };
            txtSearch = new TextBox { Location = new Point(15, 55), Width = 350 };
            txtSearch.TextChanged += (s, e) => ApplyFilter();

            Label lblFilterStat = new Label { Text = "Lọc tình trạng phiếu:", Location = new Point(15, 100), AutoSize = true };
            cboFilterStatus = new ComboBox { Location = new Point(15, 125), Width = 350, DropDownStyle = ComboBoxStyle.DropDownList };
            cboFilterStatus.Items.AddRange(new object[] { "Tất cả", "Đang mượn", "Quá hạn", "Đã trả" });
            cboFilterStatus.SelectedIndex = 0;
            cboFilterStatus.SelectedIndexChanged += (s, e) => ApplyFilter();

            grpFilter.Controls.AddRange(new Control[] { lblSearch, txtSearch, lblFilterStat, cboFilterStatus });

            topPanel.Controls.Add(grpInput);
            topPanel.Controls.Add(grpFilter);

            // DataGridView
            dgvTickets = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White
            };

            dgvTickets.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { DataPropertyName = "TicketId", HeaderText = "Mã phiếu", Width = 95 },
                new DataGridViewTextBoxColumn { DataPropertyName = "StudentId", HeaderText = "Mã SV", Width = 110 },
                new DataGridViewTextBoxColumn { DataPropertyName = "StudentName", HeaderText = "Họ tên SV", Width = 170 },
                new DataGridViewTextBoxColumn { DataPropertyName = "EquipmentName", HeaderText = "Thiết bị mượn", Width = 220 },
                new DataGridViewTextBoxColumn { DataPropertyName = "Quantity", HeaderText = "SL", Width = 50 },
                new DataGridViewTextBoxColumn { DataPropertyName = "BorrowDate", HeaderText = "Ngày mượn", Width = 105, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" } },
                new DataGridViewTextBoxColumn { DataPropertyName = "DueDate", HeaderText = "Hạn trả", Width = 105, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" } },
                new DataGridViewTextBoxColumn { DataPropertyName = "ReturnDate", HeaderText = "Ngày trả TT", Width = 110, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy", NullValue = "(Chưa trả)" } },
                new DataGridViewTextBoxColumn { DataPropertyName = "Status", HeaderText = "Tình trạng", Width = 110 }
            });

            dgvTickets.SelectionChanged += DgvTickets_SelectionChanged;

            statusStrip = new StatusStrip();
            lblSummaryStatus = new ToolStripStatusLabel();
            statusStrip.Items.Add(lblSummaryStatus);

            this.Controls.Add(dgvTickets);
            this.Controls.Add(topPanel);
            this.Controls.Add(statusStrip);
            this.Controls.Add(menuStrip);
        }

        private void SetupBindings()
        {
            bindingSource.DataSource = displayList;
            dgvTickets.DataSource = bindingSource;
        }

        private void InitSampleData()
        {
            DateTime today = DateTime.Today;
            masterList.Add(new CheckoutTicket { TicketId = "PM001", StudentId = "2481032001", StudentName = "Phạm Xuân Tiến", EquipmentName = "Bộ định tuyến Cisco 2911", Quantity = 1, BorrowDate = today.AddDays(-10), DueDate = today.AddDays(-2), ReturnDate = null });
            masterList.Add(new CheckoutTicket { TicketId = "PM002", StudentId = "2481032002", StudentName = "Nguyễn Văn Đức", EquipmentName = "Kit Raspberry Pi 4", Quantity = 2, BorrowDate = today.AddDays(-3), DueDate = today.AddDays(4), ReturnDate = null });
            masterList.Add(new CheckoutTicket { TicketId = "PM003", StudentId = "2481032003", StudentName = "Trần Hoàng Long", EquipmentName = "Switch Cisco 2960", Quantity = 1, BorrowDate = today.AddDays(-7), DueDate = today.AddDays(-1), ReturnDate = today.AddDays(-1) });
        }

        private void UpdateAllTicketStatuses()
        {
            DateTime today = DateTime.Today;
            foreach (var t in masterList)
            {
                if (t.ReturnDate.HasValue)
                {
                    t.Status = "Đã trả";
                }
                else if (t.DueDate.Date < today)
                {
                    t.Status = "Quá hạn";
                }
                else
                {
                    t.Status = "Đang mượn";
                }
            }
        }

        private void DgvTickets_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvTickets.CurrentRow?.DataBoundItem is CheckoutTicket selected)
            {
                txtTicketId.Text = selected.TicketId;
                txtTicketId.ReadOnly = true;
                txtStudentId.Text = selected.StudentId;
                txtStudentName.Text = selected.StudentName;
                cboEquipment.SelectedItem = selected.EquipmentName;
                txtQuantity.Text = selected.Quantity.ToString();
                dtpBorrowDate.Value = selected.BorrowDate;
                dtpDueDate.Value = selected.DueDate;
                if (selected.ReturnDate.HasValue)
                {
                    chkReturned.Checked = true;
                    dtpReturnDate.Value = selected.ReturnDate.Value;
                }
                else
                {
                    chkReturned.Checked = false;
                    dtpReturnDate.Value = DateTime.Today;
                }
                cboStatus.SelectedItem = selected.Status;
                errorProvider.Clear();
            }
        }

        private bool ValidateData(bool isNew)
        {
            errorProvider.Clear();
            bool valid = true;

            string tid = txtTicketId.Text.Trim();
            if (string.IsNullOrWhiteSpace(tid))
            {
                errorProvider.SetError(txtTicketId, "Mã phiếu không được để trống!");
                valid = false;
            }
            else if (isNew && masterList.Any(t => t.TicketId.Equals(tid, StringComparison.OrdinalIgnoreCase)))
            {
                errorProvider.SetError(txtTicketId, "Mã phiếu mượn đã tồn tại!");
                valid = false;
            }

            if (string.IsNullOrWhiteSpace(txtStudentId.Text))
            {
                errorProvider.SetError(txtStudentId, "Mã sinh viên không được để trống!");
                valid = false;
            }

            if (string.IsNullOrWhiteSpace(txtStudentName.Text))
            {
                errorProvider.SetError(txtStudentName, "Họ tên sinh viên không được để trống!");
                valid = false;
            }

            if (!int.TryParse(txtQuantity.Text.Trim(), out int qty) || qty <= 0)
            {
                errorProvider.SetError(txtQuantity, "Số lượng mượn phải là số nguyên lớn hơn 0!");
                valid = false;
            }

            if (dtpDueDate.Value.Date < dtpBorrowDate.Value.Date)
            {
                errorProvider.SetError(dtpDueDate, "Hạn trả không được trước ngày mượn!");
                valid = false;
            }

            if (chkReturned.Checked && dtpReturnDate.Value.Date < dtpBorrowDate.Value.Date)
            {
                errorProvider.SetError(dtpReturnDate, "Ngày trả thực tế không được trước ngày mượn!");
                valid = false;
            }

            return valid;
        }

        private void AddTicket()
        {
            if (!ValidateData(isNew: true)) return;

            var ticket = new CheckoutTicket
            {
                TicketId = txtTicketId.Text.Trim(),
                StudentId = txtStudentId.Text.Trim(),
                StudentName = txtStudentName.Text.Trim(),
                EquipmentName = cboEquipment.SelectedItem?.ToString() ?? "",
                Quantity = int.Parse(txtQuantity.Text.Trim()),
                BorrowDate = dtpBorrowDate.Value.Date,
                DueDate = dtpDueDate.Value.Date,
                ReturnDate = chkReturned.Checked ? dtpReturnDate.Value.Date : (DateTime?)null
            };

            masterList.Add(ticket);
            UpdateAllTicketStatuses();
            ApplyFilter();
            MessageBox.Show("Lập phiếu mượn thiết bị thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ResetFields();
        }

        private void UpdateTicket()
        {
            string tid = txtTicketId.Text.Trim();
            var existing = masterList.FirstOrDefault(t => t.TicketId.Equals(tid, StringComparison.OrdinalIgnoreCase));
            if (existing == null)
            {
                MessageBox.Show("Vui lòng chọn phiếu mượn cần cập nhật!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ValidateData(isNew: false)) return;

            existing.StudentId = txtStudentId.Text.Trim();
            existing.StudentName = txtStudentName.Text.Trim();
            existing.EquipmentName = cboEquipment.SelectedItem?.ToString() ?? "";
            existing.Quantity = int.Parse(txtQuantity.Text.Trim());
            existing.BorrowDate = dtpBorrowDate.Value.Date;
            existing.DueDate = dtpDueDate.Value.Date;
            existing.ReturnDate = chkReturned.Checked ? dtpReturnDate.Value.Date : (DateTime?)null;

            UpdateAllTicketStatuses();
            ApplyFilter();
            MessageBox.Show("Cập nhật phiếu mượn thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void DeleteTicket()
        {
            string tid = txtTicketId.Text.Trim();
            var existing = masterList.FirstOrDefault(t => t.TicketId.Equals(tid, StringComparison.OrdinalIgnoreCase));
            if (existing == null)
            {
                MessageBox.Show("Vui lòng chọn phiếu mượn cần xóa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var dr = MessageBox.Show($"Xác nhận xóa phiếu mượn [{existing.TicketId}]?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr == DialogResult.Yes)
            {
                masterList.Remove(existing);
                UpdateAllTicketStatuses();
                ApplyFilter();
                ResetFields();
                MessageBox.Show("Đã xóa phiếu mượn!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void MarkAsReturned()
        {
            string tid = txtTicketId.Text.Trim();
            var existing = masterList.FirstOrDefault(t => t.TicketId.Equals(tid, StringComparison.OrdinalIgnoreCase));
            if (existing == null)
            {
                MessageBox.Show("Vui lòng chọn phiếu mượn để đánh dấu đã trả!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            existing.ReturnDate = DateTime.Today;
            existing.Status = "Đã trả";
            chkReturned.Checked = true;
            dtpReturnDate.Value = DateTime.Today;
            cboStatus.SelectedItem = "Đã trả";

            UpdateAllTicketStatuses();
            ApplyFilter();
            MessageBox.Show($"Phiếu [{existing.TicketId}] đã được ghi nhận hoàn trả vào ngày hôm nay!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ResetFields()
        {
            txtTicketId.ReadOnly = false;
            txtTicketId.Clear();
            txtStudentId.Clear();
            txtStudentName.Clear();
            cboEquipment.SelectedIndex = 0;
            txtQuantity.Text = "1";
            dtpBorrowDate.Value = DateTime.Today;
            dtpDueDate.Value = DateTime.Today.AddDays(7);
            chkReturned.Checked = false;
            cboStatus.SelectedIndex = 0;
            errorProvider.Clear();
            txtTicketId.Focus();
        }

        private void ApplyFilter()
        {
            string keyword = txtSearch.Text.Trim().ToLower();
            string statusFilter = cboFilterStatus.SelectedItem?.ToString() ?? "Tất cả";

            var filtered = masterList.Where(t =>
                (string.IsNullOrEmpty(keyword) || t.StudentId.ToLower().Contains(keyword) || t.EquipmentName.ToLower().Contains(keyword)) &&
                (statusFilter == "Tất cả" || t.Status == statusFilter)
            ).ToList();

            displayList.Clear();
            foreach (var item in filtered) displayList.Add(item);

            int totalOverdue = masterList.Count(t => t.Status == "Quá hạn");
            lblSummaryStatus.Text = $"Tổng số phiếu: {masterList.Count} | Đang hiển thị: {displayList.Count} | Số phiếu quá hạn: {totalOverdue}";
        }

        private void ExportToCsv()
        {
            using SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "CSV File (*.csv)|*.csv",
                FileName = $"MuonTraThietBi_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("Mã phiếu,Mã SV,Họ tên SV,Thiết bị,Số lượng,Ngày mượn,Hạn trả,Ngày trả TT,Tình trạng");
                    foreach (var t in displayList)
                    {
                        string returnStr = t.ReturnDate.HasValue ? t.ReturnDate.Value.ToString("dd/MM/yyyy") : "Chưa trả";
                        sb.AppendLine($"\"{t.TicketId}\",\"{t.StudentId}\",\"{t.StudentName}\",\"{t.EquipmentName}\",{t.Quantity},\"{t.BorrowDate:dd/MM/yyyy}\",\"{t.DueDate:dd/MM/yyyy}\",\"{returnStr}\",\"{t.Status}\"");
                    }
                    File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("Xuất danh sách ra CSV thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi ghi file: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}