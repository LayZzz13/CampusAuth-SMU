using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace CampusAuth {
    public class MainForm : Form {
        private AppConfig config;
        private SrunClient client;

        // UI Controls
        private Panel pnlNotice;
        private TextBox txtServer;
        private TextBox txtUsername;
        private TextBox txtPassword;
        private CheckBox chkShowPassword;
        private ComboBox cmbDomain;
        private Label lblLocalIp;
        private CheckBox chkAutoExit;
        private Button btnLogin;
        private Button btnLogout;
        private Button btnCheck;
        private Button btnSave;
        private Button btnAutoStart;
        private TextBox txtLog;
        private Label lblStatus;
        private Panel statusIndicator;

        public MainForm() {
            InitializeComponent();
            config = AppConfig.Load();
            client = new SrunClient(config.ServerIp, config.AcId);
            LoadConfigToUI();
        }

        private void InitializeComponent() {
            this.Text = "西南民族大学-校园网认证助手 v1.0.2  @小刺猬 ";
            this.Size = new Size(540, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(248, 249, 250);
            this.Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Regular);

            try {
                this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            } catch { }

            // Header Banner
            var pnlHeader = new Panel {
                Location = new Point(0, 0),
                Size = new Size(540, 68),
                BackColor = Color.FromArgb(24, 119, 242)
            };
            var lblTitle = new Label {
                Text = "西南民族大学 校园网认证助手 v1.0.2",
                Font = new Font("Microsoft YaHei UI", 13F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 10),
                AutoSize = true
            };
            var lblSubTitle = new Label {
                Text = "作者：小刺猬  |  微信：z2531411694l  |  开机后台静默联网",
                Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(220, 235, 255),
                Location = new Point(22, 38),
                AutoSize = true
            };
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblSubTitle);
            this.Controls.Add(pnlHeader);

            // Closable Reminder Banner
            pnlNotice = new Panel {
                Location = new Point(18, 76),
                Size = new Size(490, 32),
                BackColor = Color.FromArgb(254, 243, 224),
                BorderStyle = BorderStyle.FixedSingle
            };
            var lblNoticeIcon = new Label {
                Text = "📢 提示：最新版本更新地址：",
                Location = new Point(8, 6),
                AutoSize = true,
                Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(190, 80, 0)
            };
            var linkNotice = new LinkLabel {
                Text = "https://link3.cc/zscc5401",
                Location = new Point(190, 6),
                AutoSize = true,
                Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold),
                LinkColor = Color.FromArgb(24, 119, 242)
            };
            linkNotice.LinkClicked += (s, e) => {
                try {
                    System.Diagnostics.Process.Start("https://link3.cc/zscc5401");
                } catch {
                    // ignore
                }
            };
            var btnCloseNotice = new Button {
                Text = "✕ 关闭",
                Location = new Point(415, 3),
                Size = new Size(65, 24),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Microsoft YaHei UI", 8F),
                ForeColor = Color.FromArgb(120, 120, 120),
                BackColor = Color.FromArgb(254, 243, 224),
                Cursor = Cursors.Hand
            };
            btnCloseNotice.FlatAppearance.BorderSize = 0;
            btnCloseNotice.Click += (s, e) => {
                pnlNotice.Visible = false;
            };
            pnlNotice.Controls.Add(lblNoticeIcon);
            pnlNotice.Controls.Add(linkNotice);
            pnlNotice.Controls.Add(btnCloseNotice);
            this.Controls.Add(pnlNotice);

            // Config Card
            var grpConfig = new GroupBox {
                Text = " 认证配置与账号 ",
                Location = new Point(18, 116),
                Size = new Size(490, 180),
                ForeColor = Color.FromArgb(60, 64, 67)
            };

            int labelX = 20, inputX = 120, rowY = 26, stepY = 34;

            // Server
            var lblServer = new Label { Text = "认证服务器:", Location = new Point(labelX, rowY + 3), AutoSize = true };
            txtServer = new TextBox { Text = "211.83.241.102", Location = new Point(inputX, rowY), Width = 180 };
            lblLocalIp = new Label { Text = "本机IP: 检测中...", Location = new Point(315, rowY + 3), AutoSize = true, ForeColor = Color.Gray };
            grpConfig.Controls.Add(lblServer);
            grpConfig.Controls.Add(txtServer);
            grpConfig.Controls.Add(lblLocalIp);

            // Username
            rowY += stepY;
            var lblUsername = new Label { Text = "认证账号:", Location = new Point(labelX, rowY + 3), AutoSize = true };
            txtUsername = new TextBox { Location = new Point(inputX, rowY), Width = 180 };
            grpConfig.Controls.Add(lblUsername);
            grpConfig.Controls.Add(txtUsername);

            // Domain
            var lblDomain = new Label { Text = "运营商:", Location = new Point(315, rowY + 3), AutoSize = true };
            cmbDomain = new ComboBox {
                Location = new Point(370, rowY),
                Width = 100,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbDomain.Items.AddRange(new object[] {
                "中国电信 (@telecom)",
                "中国移动 (@cmccgx-test)",
                "中国联通 (@cucc)",
                "校园网 (@swun)"
            });
            cmbDomain.SelectedIndex = 0;
            grpConfig.Controls.Add(lblDomain);
            grpConfig.Controls.Add(cmbDomain);

            // Password
            rowY += stepY;
            var lblPassword = new Label { Text = "认证密码:", Location = new Point(labelX, rowY + 3), AutoSize = true };
            txtPassword = new TextBox { Location = new Point(inputX, rowY), Width = 180, UseSystemPasswordChar = true };
            chkShowPassword = new CheckBox { Text = "显示密码", Location = new Point(315, rowY + 2), AutoSize = true };
            chkShowPassword.CheckedChanged += (s, e) => { txtPassword.UseSystemPasswordChar = !chkShowPassword.Checked; };
            grpConfig.Controls.Add(lblPassword);
            grpConfig.Controls.Add(txtPassword);
            grpConfig.Controls.Add(chkShowPassword);

            // AutoExit Checkbox
            rowY += stepY;
            chkAutoExit = new CheckBox {
                Text = "开机静默认证成功后自动退出程序 (免常驻占用内存)",
                Location = new Point(inputX, rowY),
                AutoSize = true,
                Checked = true
            };
            grpConfig.Controls.Add(chkAutoExit);

            this.Controls.Add(grpConfig);

            // Row 1 Buttons
            btnLogin = new Button {
                Text = "一键登录 / 认证",
                Location = new Point(18, 306),
                Size = new Size(130, 36),
                BackColor = Color.FromArgb(24, 119, 242),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += async (s, e) => await DoLoginAsync();
            this.Controls.Add(btnLogin);

            btnLogout = new Button {
                Text = "注销 / 下线",
                Location = new Point(158, 306),
                Size = new Size(100, 36),
                BackColor = Color.FromArgb(235, 238, 242),
                ForeColor = Color.FromArgb(60, 64, 67),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnLogout.FlatAppearance.BorderColor = Color.FromArgb(200, 205, 210);
            btnLogout.Click += async (s, e) => await DoLogoutAsync();
            this.Controls.Add(btnLogout);

            btnCheck = new Button {
                Text = "检查状态",
                Location = new Point(268, 306),
                Size = new Size(100, 36),
                BackColor = Color.FromArgb(235, 238, 242),
                ForeColor = Color.FromArgb(60, 64, 67),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCheck.FlatAppearance.BorderColor = Color.FromArgb(200, 205, 210);
            btnCheck.Click += async (s, e) => await CheckStatusAsync();
            this.Controls.Add(btnCheck);

            btnSave = new Button {
                Text = "保存设置",
                Location = new Point(378, 306),
                Size = new Size(130, 36),
                BackColor = Color.FromArgb(240, 240, 240),
                ForeColor = Color.FromArgb(40, 40, 40),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderColor = Color.FromArgb(200, 205, 210);
            btnSave.Click += (s, e) => SaveConfigFromUI(true);
            this.Controls.Add(btnSave);

            // Row 2: Dedicated AutoStart Button
            btnAutoStart = new Button {
                Text = "🚀 开机自启动：配置中...",
                Location = new Point(18, 350),
                Size = new Size(490, 38),
                BackColor = Color.FromArgb(232, 245, 233),
                ForeColor = Color.FromArgb(46, 125, 50),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnAutoStart.FlatAppearance.BorderColor = Color.FromArgb(165, 214, 167);
            btnAutoStart.Click += (s, e) => ToggleAutoStart();
            this.Controls.Add(btnAutoStart);

            // Status indicator
            statusIndicator = new Panel {
                Location = new Point(20, 400),
                Size = new Size(10, 10),
                BackColor = Color.Gray
            };
            lblStatus = new Label {
                Text = "状态：检测中...",
                Location = new Point(36, 396),
                AutoSize = true,
                Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Bold)
            };
            this.Controls.Add(statusIndicator);
            this.Controls.Add(lblStatus);

            // Log box
            txtLog = new TextBox {
                Location = new Point(18, 420),
                Size = new Size(490, 175),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.FromArgb(250, 252, 255),
                Font = new Font("Consolas", 9F)
            };
            this.Controls.Add(txtLog);

            this.Load += async (s, e) => {
                string ip = client.GetBestLocalIp();
                lblLocalIp.Text = "本机IP: " + ip;
                Log("程序启动。已识别校园网适配器 IP: " + ip);
                UpdateAutoStartButton();
                await CheckStatusAsync();
            };
        }

        private void LoadConfigToUI() {
            txtServer.Text = config.ServerIp;
            txtUsername.Text = config.Username;
            txtPassword.Text = config.Password;

            if (config.Domain.Contains("cmcc")) cmbDomain.SelectedIndex = 1;
            else if (config.Domain.Contains("cucc")) cmbDomain.SelectedIndex = 2;
            else if (config.Domain.Contains("swun")) cmbDomain.SelectedIndex = 3;
            else cmbDomain.SelectedIndex = 0; // telecom

            chkAutoExit.Checked = config.AutoExitOnSuccess;
        }

        private void UpdateAutoStartButton() {
            bool isRegistered = AppConfig.CheckAutoStartRegistry();
            config.AutoStart = isRegistered;
            if (isRegistered) {
                btnAutoStart.Text = "✔ 开机自启动权限：已启用 (点击可切换关闭)";
                btnAutoStart.BackColor = Color.FromArgb(232, 245, 233);
                btnAutoStart.ForeColor = Color.FromArgb(46, 125, 50);
                btnAutoStart.FlatAppearance.BorderColor = Color.FromArgb(165, 214, 167);
            } else {
                btnAutoStart.Text = "➕ 点击获取开机自启动权限 (开机后台静默联网)";
                btnAutoStart.BackColor = Color.FromArgb(254, 243, 224);
                btnAutoStart.ForeColor = Color.FromArgb(230, 81, 0);
                btnAutoStart.FlatAppearance.BorderColor = Color.FromArgb(255, 204, 128);
            }
        }

        private void ToggleAutoStart() {
            bool current = AppConfig.CheckAutoStartRegistry();
            bool target = !current;
            SaveConfigFromUI(false);
            AppConfig.UpdateAutoStartRegistry(target);
            config.AutoStart = target;
            config.Save();
            UpdateAutoStartButton();

            if (target) {
                MessageBox.Show(
                    this,
                    "已成功获取并绑定【开机自启动】权限！\n\n以后每次打开电脑，程序将在后台静默完成校园网认证，并在认证完成后自动退出，无需手动操作。",
                    "开机自启动已启用",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                Log("已开启开机自启动权限。");
            } else {
                MessageBox.Show(
                    this,
                    "已关闭开机自启动权限。",
                    "提示",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                Log("已移除开机自启动权限。");
            }
        }

        private string GetSelectedDomain() {
            switch (cmbDomain.SelectedIndex) {
                case 1: return "@cmccgx-test";
                case 2: return "@cucc";
                case 3: return "@swun";
                default: return "@telecom";
            }
        }

        private void SaveConfigFromUI(bool showMsg = false) {
            config.ServerIp = txtServer.Text.Trim();
            config.Username = txtUsername.Text.Trim();
            config.Password = txtPassword.Text;
            config.Domain = GetSelectedDomain();
            config.AutoExitOnSuccess = chkAutoExit.Checked;
            config.AutoStart = AppConfig.CheckAutoStartRegistry();
            config.Save();

            client.ServerIp = config.ServerIp;
            Log("设置已保存，账号: " + (string.IsNullOrEmpty(config.Username) ? "(未设置)" : config.Username + config.Domain));
            if (showMsg) {
                MessageBox.Show(this, "设置保存成功！下次打开软件将自动填入已保存的账号密码。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void Log(string msg) {
            string time = DateTime.Now.ToString("HH:mm:ss");
            txtLog.AppendText(string.Format("[{0}] {1}\r\n", time, msg));
        }

        private void SetStatus(bool online, string text) {
            statusIndicator.BackColor = online ? Color.FromArgb(40, 167, 69) : Color.FromArgb(220, 53, 69);
            lblStatus.Text = "状态：" + text;
            lblStatus.ForeColor = online ? Color.FromArgb(40, 167, 69) : Color.FromArgb(220, 53, 69);
        }

        private async Task CheckStatusAsync() {
            SetStatus(false, "正在检测...");
            btnCheck.Enabled = false;
            try {
                var res = await Task.Run(() => client.CheckOnlineStatus());
                if (res.Success) {
                    SetStatus(true, "已在线");
                    Log("状态检测结果: " + res.Message);
                } else {
                    SetStatus(false, "未在线");
                    Log("状态检测结果: " + res.Message);
                }
            } catch (Exception ex) {
                SetStatus(false, "检测失败");
                Log("检测异常: " + ex.Message);
            } finally {
                btnCheck.Enabled = true;
            }
        }

        private async Task DoLoginAsync() {
            string user = txtUsername.Text.Trim();
            string pwd = txtPassword.Text;
            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pwd)) {
                MessageBox.Show(this, "请输入校园网认证账号与密码！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
                return;
            }

            SaveConfigFromUI(false);
            btnLogin.Enabled = false;
            Log("开始发起认证登录流程...");
            SetStatus(false, "正在认证中...");

            try {
                string domain = GetSelectedDomain();

                var res = await Task.Run(() => client.Login(user, domain, pwd));
                if (res.Success) {
                    Log("★ 认证结果: " + res.Message);
                    await CheckStatusAsync();
                    MessageBox.Show(this, res.Message, "认证成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                } else {
                    SetStatus(false, "认证失败");
                    Log("✗ 认证失败: " + res.Message);
                    MessageBox.Show(this, res.Message, "认证失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            } catch (Exception ex) {
                SetStatus(false, "认证异常");
                Log("异常: " + ex.Message);
                MessageBox.Show(this, "认证异常: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            } finally {
                btnLogin.Enabled = true;
            }
        }

        private async Task DoLogoutAsync() {
            btnLogout.Enabled = false;
            Log("正在发起注销下线请求...");
            SetStatus(false, "正在注销下线...");
            try {
                string user = txtUsername.Text.Trim();
                string domain = GetSelectedDomain();

                var res = await Task.Run(() => client.Logout(user, domain));
                Log("注销请求已发出，正在等待网关更新会话...");

                // Wait 1.5 seconds for gateway session to flush
                await Task.Delay(1500);

                await CheckStatusAsync();
                MessageBox.Show(this, "已成功注销下线！", "注销成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            } catch (Exception ex) {
                Log("注销异常: " + ex.Message);
            } finally {
                btnLogout.Enabled = true;
            }
        }
    }
}
