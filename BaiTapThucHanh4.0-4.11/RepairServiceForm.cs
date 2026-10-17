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
    public class RepairOrder
    {
        public string OrderId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string DeviceType { get; set; } = string.Empty;
        public string ErrorDescription { get; set; } = string.Empty;
        public DateTime ReceiveDate { get; set; }
        public decimal EstimatedCost { get; set; }
        public string Status { get; set; } = "Mới tiếp nhận";
        public string ImagePath { get; set; } = string.Empty;
    }

    public class RepairServiceForm : Form
    {
        private TextBox txtOrderId = null!, txtCustomerName = null!, txtPhone = null!, txtErrorDesc = null!, txtCost = null!, txtSearch = null!;
        private ComboBox cboDeviceType = null!, cboStatus = null!, cboFilterStatus = null!;
        private DateTimePicker dtpReceiveDate = null!;
        private PictureBox picDevice = null!;
        private Button btnChooseImage = null!, btnRemoveImage = null!, btnAdd = null!, btnUpdate = null!, btnDelete = null!, btnReset = null!;
        private DataGridView dgvOrders = null!;
        private StatusStrip statusStrip = null!;
        private ToolStripStatusLabel lblSummaryStatus = null!;
        private MenuStrip menuStrip = null!;
        private ErrorProvider errorProvider = null!;

        private string currentImagePath = string.Empty;
        private BindingList<RepairOrder> masterList = new BindingList<RepairOrder>();
        private BindingList<RepairOrder> displayList = new BindingList<RepairOrder>();
        private BindingSource bindingSource = new BindingSource();

        public RepairServiceForm()
        {
            SetupForm();
            SetupControls();
            SetupBindings();
            InitSampleData();
            ApplyFilter();
        }

        private void SetupForm()
        {
            this.Text = "Repair Service Tracker - Quản lý đơn dịch vụ sửa chữa thiết bị";
            this.Size = new Size(1180, 750);
            this.MinimumSize = new Size(1000, 650);
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
            ToolStripMenuItem mnuExit = new ToolStripMenuItem("Thoát (&X)", null, (s, e) => Application.Exit());
            mnuFile.DropDownItems.AddRange(new ToolStripItem[] { mnuExport, new ToolStripSeparator(), mnuExit });
            menuStrip.Items.Add(mnuFile);
            this.MainMenuStrip = menuStrip;

            // Panel nhập liệu phía trên
            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 290, Padding = new Padding(10) };

            GroupBox grpInfo = new GroupBox
            {
                Text = "Chi tiết đơn sửa chữa",
                Location = new Point(10, 5),
                Size = new Size(580, 275),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            Label lblId = new Label { Text = "Mã đơn:", Location = new Point(15, 28), AutoSize = true };
            txtOrderId = new TextBox { Location = new Point(90, 25), Width = 150 };

            Label lblCust = new Label { Text = "Khách hàng:", Location = new Point(260, 28), AutoSize = true };
            txtCustomerName = new TextBox { Location = new Point(345, 25), Width = 210 };

            Label lblPhone = new Label { Text = "Số ĐT:", Location = new Point(15, 65), AutoSize = true };
            txtPhone = new TextBox { Location = new Point(90, 62), Width = 150 };

            Label lblDevice = new Label { Text = "Loại thiết bị:", Location = new Point(260, 65), AutoSize = true };
            cboDeviceType = new ComboBox { Location = new Point(345, 62), Width = 210, DropDownStyle = ComboBoxStyle.DropDownList };
            cboDeviceType.Items.AddRange(new object[] { "Laptop", "Máy tính bảng (Tablet)", "Điện thoại thông minh", "Màn hình PC", "Mainboard / Card đồ họa", "Nguồn / UPS" });
            cboDeviceType.SelectedIndex = 0;

            Label lblDate = new Label { Text = "Ngày nhận:", Location = new Point(15, 102), AutoSize = true };
            dtpReceiveDate = new DateTimePicker { Location = new Point(90, 99), Width = 150, Format = DateTimePickerFormat.Short };

            Label lblCost = new Label { Text = "Chi phí (VNĐ):", Location = new Point(260, 102), AutoSize = true };
            txtCost = new TextBox { Location = new Point(345, 99), Width = 210, Text = "0" };

            Label lblStat = new Label { Text = "Trạng thái:", Location = new Point(15, 139), AutoSize = true };
            cboStatus = new ComboBox { Location = new Point(90, 136), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cboStatus.Items.AddRange(new object[] { "Mới tiếp nhận", "Đang sửa", "Chờ linh kiện", "Hoàn tất", "Đã hủy" });
            cboStatus.SelectedIndex = 0;

            Label lblDesc = new Label { Text = "Mô tả lỗi:", Location = new Point(15, 176), AutoSize = true };
            txtErrorDesc = new TextBox { Location = new Point(90, 173), Width = 465 };

            btnAdd = new Button { Text = "Thêm đơn", Location = new Point(90, 220), Size = new Size(95, 34), BackColor = Color.FromArgb(0, 120, 215), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnUpdate = new Button { Text = "Cập nhật", Location = new Point(195, 220), Size = new Size(95, 34) };
            btnDelete = new Button { Text = "Xóa đơn", Location = new Point(300, 220), Size = new Size(95, 34), BackColor = Color.FromArgb(217, 83, 79), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnReset = new Button { Text = "Làm mới", Location = new Point(405, 220), Size = new Size(95, 34) };

            btnAdd.Click += (s, e) => AddOrder();
            btnUpdate.Click += (s, e) => UpdateOrder();
            btnDelete.Click += (s, e) => DeleteOrder();
            btnReset.Click += (s, e) => ResetFields();

            grpInfo.Controls.AddRange(new Control[] {
                lblId, txtOrderId, lblCust, txtCustomerName, lblPhone, txtPhone,
                lblDevice, cboDeviceType, lblDate, dtpReceiveDate, lblCost, txtCost,
                lblStat, cboStatus, lblDesc, txtErrorDesc,
                btnAdd, btnUpdate, btnDelete, btnReset
            });

            // GroupBox Ảnh thiết bị
            GroupBox grpImage = new GroupBox
            {
                Text = "Ảnh thiết bị lỗi",
                Location = new Point(600, 5),
                Size = new Size(230, 275),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            picDevice = new PictureBox
            {
                Location = new Point(15, 25),
                Size = new Size(200, 185),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.WhiteSmoke
            };

            btnChooseImage = new Button { Text = "Chọn ảnh...", Location = new Point(15, 225), Size = new Size(95, 30) };
            btnRemoveImage = new Button { Text = "Xóa ảnh", Location = new Point(120, 225), Size = new Size(95, 30) };

            btnChooseImage.Click += BtnChooseImage_Click;
            btnRemoveImage.Click += (s, e) => { currentImagePath = string.Empty; picDevice.Image = null; };

            grpImage.Controls.AddRange(new Control[] { picDevice, btnChooseImage, btnRemoveImage });

            // GroupBox Tìm kiếm & Lọc
            GroupBox grpFilter = new GroupBox
            {
                Text = "Tìm kiếm & Lọc",
                Location = new Point(840, 5),
                Size = new Size(320, 275),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            Label lblSearch = new Label { Text = "Tìm (Mã đơn / Tên KH / SĐT):", Location = new Point(15, 30), AutoSize = true };
            txtSearch = new TextBox { Location = new Point(15, 55), Width = 285 };
            txtSearch.TextChanged += (s, e) => ApplyFilter();

            Label lblFStat = new Label { Text = "Lọc theo trạng thái:", Location = new Point(15, 105), AutoSize = true };
            cboFilterStatus = new ComboBox { Location = new Point(15, 130), Width = 285, DropDownStyle = ComboBoxStyle.DropDownList };
            cboFilterStatus.Items.AddRange(new object[] { "Tất cả", "Mới tiếp nhận", "Đang sửa", "Chờ linh kiện", "Hoàn tất", "Đã hủy" });
            cboFilterStatus.SelectedIndex = 0;
            cboFilterStatus.SelectedIndexChanged += (s, e) => ApplyFilter();

            grpFilter.Controls.AddRange(new Control[] { lblSearch, txtSearch, lblFStat, cboFilterStatus });

            topPanel.Controls.Add(grpInfo);
            topPanel.Controls.Add(grpImage);
            topPanel.Controls.Add(grpFilter);

            // DataGridView
            dgvOrders = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White
            };

            dgvOrders.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { DataPropertyName = "OrderId", HeaderText = "Mã đơn", Width = 90 },
                new DataGridViewTextBoxColumn { DataPropertyName = "CustomerName", HeaderText = "Khách hàng", Width = 160 },
                new DataGridViewTextBoxColumn { DataPropertyName = "PhoneNumber", HeaderText = "Số ĐT", Width = 110 },
                new DataGridViewTextBoxColumn { DataPropertyName = "DeviceType", HeaderText = "Loại thiết bị", Width = 150 },
                new DataGridViewTextBoxColumn { DataPropertyName = "ReceiveDate", HeaderText = "Ngày nhận", Width = 105, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" } },
                new DataGridViewTextBoxColumn { DataPropertyName = "EstimatedCost", HeaderText = "Chi phí dự kiến", Width = 130, DefaultCellStyle = new DataGridViewCellStyle { Format = "N0", Alignment = DataGridViewContentAlignment.MiddleRight } },
                new DataGridViewTextBoxColumn { DataPropertyName = "Status", HeaderText = "Trạng thái", Width = 110 },
                new DataGridViewTextBoxColumn { DataPropertyName = "ErrorDescription", HeaderText = "Mô tả lỗi", Width = 230 }
            });

            dgvOrders.SelectionChanged += DgvOrders_SelectionChanged;

            statusStrip = new StatusStrip();
            lblSummaryStatus = new ToolStripStatusLabel();
            statusStrip.Items.Add(lblSummaryStatus);

            this.Controls.Add(dgvOrders);
            this.Controls.Add(topPanel);
            this.Controls.Add(statusStrip);
            this.Controls.Add(menuStrip);
        }

        private void SetupBindings()
        {
            bindingSource.DataSource = displayList;
            dgvOrders.DataSource = bindingSource;
        }

        private void InitSampleData()
        {
            DateTime now = DateTime.Today;
            masterList.Add(new RepairOrder { OrderId = "SC001", CustomerName = "Lê Hồng Quân", PhoneNumber = "0977112233", DeviceType = "Laptop", ErrorDescription = "Bật không lên nguồn, chập cổng Type-C", ReceiveDate = now.AddDays(-2), EstimatedCost = 1200000, Status = "Đang sửa" });
            masterList.Add(new RepairOrder { OrderId = "SC002", CustomerName = "Trần Thị Hạnh", PhoneNumber = "0988445566", DeviceType = "Điện thoại thông minh", ErrorDescription = "Vỡ màn hình OLED, cảm ứng loạn", ReceiveDate = now.AddDays(-1), EstimatedCost = 850000, Status = "Chờ linh kiện" });
            masterList.Add(new RepairOrder { OrderId = "SC003", CustomerName = "Vũ Đình Trọng", PhoneNumber = "0911778899", DeviceType = "Mainboard / Card đồ họa", ErrorDescription = "Quạt GPU kêu to, sập nguồn khi chạy nặng", ReceiveDate = now.AddDays(-5), EstimatedCost = 450000, Status = "Hoàn tất" });
        }

        private void DgvOrders_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvOrders.CurrentRow?.DataBoundItem is RepairOrder selected)
            {
                txtOrderId.Text = selected.OrderId;
                txtOrderId.ReadOnly = true;
                txtCustomerName.Text = selected.CustomerName;
                txtPhone.Text = selected.PhoneNumber;
                cboDeviceType.SelectedItem = selected.DeviceType;
                dtpReceiveDate.Value = selected.ReceiveDate;
                txtCost.Text = selected.EstimatedCost.ToString("0");
                cboStatus.SelectedItem = selected.Status;
                txtErrorDesc.Text = selected.ErrorDescription;
                currentImagePath = selected.ImagePath;

                if (!string.IsNullOrEmpty(currentImagePath) && File.Exists(currentImagePath))
                {
                    try { picDevice.Image = Image.FromFile(currentImagePath); }
                    catch { picDevice.Image = null; }
                }
                else
                {
                    picDevice.Image = null;
                }
                errorProvider.Clear();
            }
        }

        private void BtnChooseImage_Click(object? sender, EventArgs e)
        {
            using OpenFileDialog ofd = new OpenFileDialog
            {
                Title = "Chọn ảnh thiết bị hỏng",
                Filter = "Hình ảnh (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                currentImagePath = ofd.FileName;
                try
                {
                    picDevice.Image = Image.FromFile(currentImagePath);
                }
                catch
                {
                    picDevice.Image = null;
                }
            }
        }

        private bool ValidateForm(bool isNew)
        {
            errorProvider.Clear();
            bool valid = true;

            string id = txtOrderId.Text.Trim();
            if (string.IsNullOrWhiteSpace(id))
            {
                errorProvider.SetError(txtOrderId, "Mã đơn không được để trống!");
                valid = false;
            }
            else if (isNew && masterList.Any(o => o.OrderId.Equals(id, StringComparison.OrdinalIgnoreCase)))
            {
                errorProvider.SetError(txtOrderId, "Mã đơn dịch vụ đã tồn tại!");
                valid = false;
            }

            if (string.IsNullOrWhiteSpace(txtCustomerName.Text))
            {
                errorProvider.SetError(txtCustomerName, "Tên khách hàng không được để trống!");
                valid = false;
            }

            string phone = txtPhone.Text.Trim();
            if (string.IsNullOrWhiteSpace(phone) || !Regex.IsMatch(phone, @"^(0[3|5|7|8|9])+([0-9]{8})$"))
            {
                errorProvider.SetError(txtPhone, "Số điện thoại không hợp lệ (cần 10 số, đầu 03, 05, 07, 08, 09)!");
                valid = false;
            }

            if (!decimal.TryParse(txtCost.Text.Trim(), out decimal cost) || cost < 0)
            {
                errorProvider.SetError(txtCost, "Chi phí dự kiến phải là một số không âm!");
                valid = false;
            }

            if (string.IsNullOrWhiteSpace(txtErrorDesc.Text))
            {
                errorProvider.SetError(txtErrorDesc, "Bắt buộc phải nhập mô tả lỗi thiết bị!");
                valid = false;
            }

            return valid;
        }

        private void AddOrder()
        {
            if (!ValidateForm(isNew: true)) return;

            var newOrder = new RepairOrder
            {
                OrderId = txtOrderId.Text.Trim(),
                CustomerName = txtCustomerName.Text.Trim(),
                PhoneNumber = txtPhone.Text.Trim(),
                DeviceType = cboDeviceType.SelectedItem?.ToString() ?? "",
                ReceiveDate = dtpReceiveDate.Value.Date,
                EstimatedCost = decimal.Parse(txtCost.Text.Trim()),
                Status = cboStatus.SelectedItem?.ToString() ?? "Mới tiếp nhận",
                ErrorDescription = txtErrorDesc.Text.Trim(),
                ImagePath = currentImagePath
            };

            masterList.Add(newOrder);
            ApplyFilter();
            MessageBox.Show("Tiếp nhận đơn sửa chữa thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ResetFields();
        }

        private void UpdateOrder()
        {
            string id = txtOrderId.Text.Trim();
            var existing = masterList.FirstOrDefault(o => o.OrderId.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (existing == null)
            {
                MessageBox.Show("Vui lòng chọn đơn cần cập nhật từ danh sách!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ValidateForm(isNew: false)) return;

            existing.CustomerName = txtCustomerName.Text.Trim();
            existing.PhoneNumber = txtPhone.Text.Trim();
            existing.DeviceType = cboDeviceType.SelectedItem?.ToString() ?? "";
            existing.ReceiveDate = dtpReceiveDate.Value.Date;
            existing.EstimatedCost = decimal.Parse(txtCost.Text.Trim());
            existing.Status = cboStatus.SelectedItem?.ToString() ?? "Mới tiếp nhận";
            existing.ErrorDescription = txtErrorDesc.Text.Trim();
            existing.ImagePath = currentImagePath;

            ApplyFilter();
            MessageBox.Show("Cập nhật đơn sửa chữa thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void DeleteOrder()
        {
            string id = txtOrderId.Text.Trim();
            var existing = masterList.FirstOrDefault(o => o.OrderId.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (existing == null)
            {
                MessageBox.Show("Vui lòng chọn đơn cần xóa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var dr = MessageBox.Show($"Xác nhận xóa đơn sửa chữa [{existing.OrderId}]?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr == DialogResult.Yes)
            {
                masterList.Remove(existing);
                ApplyFilter();
                ResetFields();
                MessageBox.Show("Đã xóa đơn sửa chữa thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ResetFields()
        {
            txtOrderId.ReadOnly = false;
            txtOrderId.Clear();
            txtCustomerName.Clear();
            txtPhone.Clear();
            cboDeviceType.SelectedIndex = 0;
            dtpReceiveDate.Value = DateTime.Today;
            txtCost.Text = "0";
            cboStatus.SelectedIndex = 0;
            txtErrorDesc.Clear();
            currentImagePath = string.Empty;
            picDevice.Image = null;
            errorProvider.Clear();
            txtOrderId.Focus();
        }

        private void ApplyFilter()
        {
            string keyword = txtSearch.Text.Trim().ToLower();
            string statusFilter = cboFilterStatus.SelectedItem?.ToString() ?? "Tất cả";

            var filtered = masterList.Where(o =>
                (string.IsNullOrEmpty(keyword) || o.OrderId.ToLower().Contains(keyword) || o.CustomerName.ToLower().Contains(keyword) || o.PhoneNumber.Contains(keyword)) &&
                (statusFilter == "Tất cả" || o.Status == statusFilter)
            ).ToList();

            displayList.Clear();
            foreach (var item in filtered) displayList.Add(item);

            decimal pendingCost = masterList
                .Where(o => o.Status != "Hoàn tất" && o.Status != "Đã hủy")
                .Sum(o => o.EstimatedCost);

            lblSummaryStatus.Text = $"Số đơn hiển thị: {displayList.Count} / {masterList.Count} | Tổng chi phí dự kiến chưa hoàn tất: {pendingCost:N0} VNĐ";
        }

        private void ExportToCsv()
        {
            using SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "CSV File (*.csv)|*.csv",
                FileName = $"DonSuaChua_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("Mã đơn,Tên khách hàng,Số điện thoại,Loại thiết bị,Ngày nhận,Chi phí dự kiến,Trạng thái,Mô tả lỗi");
                    foreach (var o in displayList)
                    {
                        sb.AppendLine($"\"{o.OrderId}\",\"{o.CustomerName}\",\"{o.PhoneNumber}\",\"{o.DeviceType}\",\"{o.ReceiveDate:dd/MM/yyyy}\",{o.EstimatedCost},\"{o.Status}\",\"{o.ErrorDescription}\"");
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