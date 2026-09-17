using System;
using System.IO;
using System.IO.Compression;
using System.Drawing;
using System.Text;
using System.Reflection;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("西南民族大学-校园网认证助手 安装向导")]
[assembly: AssemblyDescription("西南民族大学-校园网认证助手 安装程序")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("小刺猬")]
[assembly: AssemblyProduct("西南民族大学-校园网认证助手")]
[assembly: AssemblyCopyright("Copyright © 2026 小刺猬. All Rights Reserved.")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: AssemblyVersion("1.0.2.0")]
[assembly: AssemblyFileVersion("1.0.2.0")]

namespace CampusAuthInstaller {
    public class InstallerForm : Form {
        private Panel pnlHeader;
        private Panel pnlStep1;
        private Panel pnlStep2;
        private Panel pnlStep3;

        // Step 1 Controls
        private TextBox txtAgreement;
        private CheckBox chkAgree;
        private Button btnStep1Next;
        private Button btnStep1Cancel;

        // Step 2 Controls
        private TextBox txtInstallPath;
        private Button btnBrowse;
        private CheckBox chkDesktopShortcut;
        private CheckBox chkRunAfter;
        private Button btnStep2Back;
        private Button btnStep2Install;

        // Step 3 Controls
        private ProgressBar progressBar;
        private Label lblInstallStatus;
        private Button btnFinish;

        public InstallerForm() {
            InitializeUI();
        }

        private void InitializeUI() {
            this.Text = "西南民族大学-校园网认证助手 安装向导";
            this.Size = new Size(560, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(248, 249, 250);
            this.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular);

            try {
                this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            } catch { }

            // Header Banner
            pnlHeader = new Panel {
                Location = new Point(0, 0),
                Size = new Size(560, 70),
                BackColor = Color.FromArgb(24, 119, 242)
            };
            var lblTitle = new Label {
                Text = "西南民族大学 校园网认证助手 安装向导",
                Font = new Font("Microsoft YaHei UI", 12.5F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 12),
                AutoSize = true
            };
            var lblSub = new Label {
                Text = "作者：小刺猬  |  微信：z2531411694l  |  更新地址：https://link3.cc/zscc5401",
                Font = new Font("Microsoft YaHei UI", 8.5F, FontStyle.Regular),
                ForeColor = Color.FromArgb(220, 235, 255),
                Location = new Point(22, 40),
                AutoSize = true
            };
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblSub);
            this.Controls.Add(pnlHeader);

            InitStep1();
            InitStep2();
            InitStep3();

            ShowStep(1);
        }

        private void InitStep1() {
            pnlStep1 = new Panel {
                Location = new Point(0, 70),
                Size = new Size(560, 395),
                BackColor = Color.FromArgb(248, 249, 250)
            };

            var lblStepTitle = new Label {
                Text = "用户许可协议与免责声明",
                Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold),
                Location = new Point(20, 12),
                AutoSize = true
            };
            pnlStep1.Controls.Add(lblStepTitle);

            txtAgreement = new TextBox {
                Location = new Point(20, 38),
                Size = new Size(505, 260),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.White,
                Font = new Font("Microsoft YaHei UI", 8.8F),
                Text =
@"【西南民族大学-校园网认证助手 使用协议及免责声明】

欢迎使用《西南民族大学-校园网认证助手》（以下简称“本软件”）。本软件由个人开发者“小刺猬”为方便西南民族大学师生网络接入而编写。在安装或使用本软件前，请仔细阅读以下条款：

一、软件用途与说明
1. 本软件仅用于辅助西南民族大学师生在校内网络环境下进行正常的深澜 Portal 认证与无感联网，免去频繁手动打开浏览器输入地址认证的繁琐操作。
2. 本软件为纯绿色工具，绝不包含任何恶意代码、木马后门、监控或弹窗广告。

二、隐私与安全承诺
1. 本软件所保存的校园网账号、密码等信息完全加密保存在用户本机的本地配置文件中，绝对不会上传或发送给任何第三方服务器。
2. 本软件仅与学校官方认证网关（211.83.241.102）进行标准 HTTP/JSONP 认证交互。

三、用户守则与免责声明
1. 用户在使用本软件时，必须严格遵守《中华人民共和国网络安全法》以及《西南民族大学校园网络安全管理规定》，不得将本软件用于任何违规、违法或有害校园网络的行为。
2. 因用户个人账号欠费、密码错误或学校认证系统升级导致的认证失败，开发者不承担连带责任。
3. 软件交流、反馈及后续版本更新均由作者维护。

作者：小刺猬
联系微信：z2531411694l
官方发布与更新地址：https://link3.cc/zscc5401
"
            };
            pnlStep1.Controls.Add(txtAgreement);

            chkAgree = new CheckBox {
                Text = "我已仔细阅读并同意上述《用户许可协议与免责声明》",
                Location = new Point(20, 310),
                AutoSize = true,
                Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold)
            };
            chkAgree.CheckedChanged += (s, e) => {
                btnStep1Next.Enabled = chkAgree.Checked;
            };
            pnlStep1.Controls.Add(chkAgree);

            btnStep1Cancel = new Button {
                Text = "取消",
                Location = new Point(335, 345),
                Size = new Size(90, 32),
                BackColor = Color.FromArgb(235, 238, 242),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnStep1Cancel.FlatAppearance.BorderColor = Color.FromArgb(200, 205, 210);
            btnStep1Cancel.Click += (s, e) => this.Close();
            pnlStep1.Controls.Add(btnStep1Cancel);

            btnStep1Next = new Button {
                Text = "下一步 >",
                Location = new Point(435, 345),
                Size = new Size(90, 32),
                BackColor = Color.FromArgb(24, 119, 242),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold),
                Enabled = false,
                Cursor = Cursors.Hand
            };
            btnStep1Next.FlatAppearance.BorderSize = 0;
            btnStep1Next.Click += (s, e) => ShowStep(2);
            pnlStep1.Controls.Add(btnStep1Next);

            this.Controls.Add(pnlStep1);
        }

        private void InitStep2() {
            pnlStep2 = new Panel {
                Location = new Point(0, 70),
                Size = new Size(560, 395),
                BackColor = Color.FromArgb(248, 249, 250),
                Visible = false
            };

            var lblStepTitle = new Label {
                Text = "选择安装位置与附加选项",
                Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold),
                Location = new Point(20, 15),
                AutoSize = true
            };
            pnlStep2.Controls.Add(lblStepTitle);

            var lblPath = new Label {
                Text = "安装目标文件夹：",
                Location = new Point(20, 48),
                AutoSize = true
            };
            pnlStep2.Controls.Add(lblPath);

            string defaultDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "CampursAuth-SMU");
            txtInstallPath = new TextBox {
                Text = defaultDir,
                Location = new Point(20, 72),
                Size = new Size(415, 25)
            };
            pnlStep2.Controls.Add(txtInstallPath);

            btnBrowse = new Button {
                Text = "浏览...",
                Location = new Point(445, 70),
                Size = new Size(80, 28),
                BackColor = Color.FromArgb(235, 238, 242),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnBrowse.FlatAppearance.BorderColor = Color.FromArgb(200, 205, 210);
            btnBrowse.Click += (s, e) => {
                using (var dlg = new FolderBrowserDialog()) {
                    dlg.Description = "请选择校园网认证助手的安装文件夹：";
                    dlg.SelectedPath = txtInstallPath.Text;
                    if (dlg.ShowDialog(this) == DialogResult.OK) {
                        txtInstallPath.Text = dlg.SelectedPath;
                    }
                }
            };
            pnlStep2.Controls.Add(btnBrowse);

            var grpOptions = new GroupBox {
                Text = " 安装附加选项 ",
                Location = new Point(20, 120),
                Size = new Size(505, 175)
            };

            chkDesktopShortcut = new CheckBox {
                Text = "创建桌面快捷方式 (方便随时打开控制面板)",
                Location = new Point(20, 35),
                AutoSize = true,
                Checked = true
            };
            grpOptions.Controls.Add(chkDesktopShortcut);

            chkRunAfter = new CheckBox {
                Text = "安装完成后立即启动程序 (首次运行输入账号并保存)",
                Location = new Point(20, 75),
                AutoSize = true,
                Checked = true
            };
            grpOptions.Controls.Add(chkRunAfter);

            var lblNotice = new Label {
                Text = "说明：安装将释放完整的认证主程序、配置文件、完整源码及说明文档。\n启动程序后可在主界面一键启用【开机自启动】功能。",
                Location = new Point(20, 120),
                Size = new Size(465, 40),
                ForeColor = Color.Gray
            };
            grpOptions.Controls.Add(lblNotice);

            pnlStep2.Controls.Add(grpOptions);

            btnStep2Back = new Button {
                Text = "< 上一步",
                Location = new Point(335, 345),
                Size = new Size(90, 32),
                BackColor = Color.FromArgb(235, 238, 242),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnStep2Back.FlatAppearance.BorderColor = Color.FromArgb(200, 205, 210);
            btnStep2Back.Click += (s, e) => ShowStep(1);
            pnlStep2.Controls.Add(btnStep2Back);

            btnStep2Install = new Button {
                Text = "开始安装",
                Location = new Point(435, 345),
                Size = new Size(90, 32),
                BackColor = Color.FromArgb(24, 119, 242),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnStep2Install.FlatAppearance.BorderSize = 0;
            btnStep2Install.Click += (s, e) => StartInstall();
            pnlStep2.Controls.Add(btnStep2Install);

            this.Controls.Add(pnlStep2);
        }

        private void InitStep3() {
            pnlStep3 = new Panel {
                Location = new Point(0, 70),
                Size = new Size(560, 395),
                BackColor = Color.FromArgb(248, 249, 250),
                Visible = false
            };

            var lblStepTitle = new Label {
                Text = "正在安装，请稍候...",
                Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold),
                Location = new Point(20, 40),
                AutoSize = true
            };
            pnlStep3.Controls.Add(lblStepTitle);

            progressBar = new ProgressBar {
                Location = new Point(20, 80),
                Size = new Size(505, 25),
                Style = ProgressBarStyle.Continuous,
                Value = 0
            };
            pnlStep3.Controls.Add(progressBar);

            lblInstallStatus = new Label {
                Text = "准备安装...",
                Location = new Point(20, 118),
                AutoSize = true,
                ForeColor = Color.FromArgb(60, 64, 67)
            };
            pnlStep3.Controls.Add(lblInstallStatus);

            btnFinish = new Button {
                Text = "完成",
                Location = new Point(435, 345),
                Size = new Size(90, 32),
                BackColor = Color.FromArgb(24, 119, 242),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold),
                Enabled = false,
                Cursor = Cursors.Hand
            };
            btnFinish.FlatAppearance.BorderSize = 0;
            btnFinish.Click += (s, e) => FinishAndLaunch();
            pnlStep3.Controls.Add(btnFinish);

            this.Controls.Add(pnlStep3);
        }

        private void ShowStep(int step) {
            pnlStep1.Visible = (step == 1);
            pnlStep2.Visible = (step == 2);
            pnlStep3.Visible = (step == 3);
        }

        private void StartInstall() {
            string installPath = txtInstallPath.Text.Trim();
            if (string.IsNullOrEmpty(installPath)) {
                MessageBox.Show(this, "请输入有效的安装路径！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ShowStep(3);
            Application.DoEvents();

            try {
                // 1. Create directory
                lblInstallStatus.Text = "正在创建安装文件夹...";
                progressBar.Value = 25;
                Application.DoEvents();
                if (!Directory.Exists(installPath)) {
                    Directory.CreateDirectory(installPath);
                }

                // 2. Extract embedded package
                lblInstallStatus.Text = "正在解压释放程序及全部工程文件...";
                progressBar.Value = 60;
                Application.DoEvents();

                ExtractPayload(installPath);

                // 3. Create desktop shortcut
                string exeDest = Path.Combine(installPath, "CampursAuth-SMU.exe");
                if (chkDesktopShortcut.Checked && File.Exists(exeDest)) {
                    lblInstallStatus.Text = "正在创建桌面快捷方式...";
                    progressBar.Value = 85;
                    Application.DoEvents();
                    CreateDesktopShortcut(exeDest, "CampusAuth-SMU");
                }

                progressBar.Value = 100;
                lblInstallStatus.Text = "🎉 安装成功！点击【完成】按钮退出安装向导。";
                lblInstallStatus.ForeColor = Color.FromArgb(46, 125, 50);
                btnFinish.Enabled = true;
            } catch (Exception ex) {
                lblInstallStatus.Text = "安装失败: " + ex.Message;
                lblInstallStatus.ForeColor = Color.Red;
                MessageBox.Show(this, "安装失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FinishAndLaunch() {
            string exeDest = Path.Combine(txtInstallPath.Text.Trim(), "CampursAuth-SMU.exe");
            if (chkRunAfter.Checked && File.Exists(exeDest)) {
                try {
                    Process.Start(new ProcessStartInfo {
                        FileName = exeDest,
                        WorkingDirectory = Path.GetDirectoryName(exeDest)
                    });
                } catch {
                    // ignore
                }
            }
            this.Close();
        }

        private void ExtractPayload(string installPath) {
            var assembly = Assembly.GetExecutingAssembly();
            string resName = null;
            foreach (var n in assembly.GetManifestResourceNames()) {
                if (n.EndsWith("payload.bin", StringComparison.OrdinalIgnoreCase)) {
                    resName = n;
                    break;
                }
            }

            if (resName == null) {
                throw new FileNotFoundException("安装包缺少必要的数据资源！");
            }

            using (var stream = assembly.GetManifestResourceStream(resName))
            using (var gz = new GZipStream(stream, CompressionMode.Decompress))
            using (var br = new BinaryReader(gz, Encoding.UTF8)) {
                int count = br.ReadInt32();
                for (int i = 0; i < count; i++) {
                    string fileName = br.ReadString();
                    int len = br.ReadInt32();
                    byte[] data = br.ReadBytes(len);

                    string dest = Path.Combine(installPath, fileName);
                    // If config.json already exists in destination, preserve user's saved credentials
                    if (fileName.Equals("config.json", StringComparison.OrdinalIgnoreCase) && File.Exists(dest)) {
                        continue;
                    }
                    File.WriteAllBytes(dest, data);
                }
            }
        }

        #region ShellLink COM Implementation
        [ComImport]
        [Guid("00021401-0000-0000-C000-000000000046")]
        private class ShellLink { }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("000214F9-0000-0000-C000-000000000046")]
        private interface IShellLinkW {
            void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, out IntPtr pfd, int fFlags);
            void GetIDList(out IntPtr ppidl);
            void SetIDList(IntPtr pidl);
            void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cch);
            void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
            void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cch);
            void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
            void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cch);
            void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
            void GetHotkey(out short pwHotkey);
            void SetHotkey(short wHotkey);
            void GetShowCmd(out int piShowCmd);
            void SetShowCmd(int iShowCmd);
            void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cch, out int piIcon);
            void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
            void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, int dwReserved);
            void Resolve(IntPtr hwnd, int fFlags);
            void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
        }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("0000010b-0000-0000-C000-000000000046")]
        private interface IPersistFile {
            void GetClassID(out Guid pClassID);
            void IsDirty();
            void Load([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, int dwMode);
            void Save([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, [MarshalAs(UnmanagedType.Bool)] bool fRemember);
            void SaveCompleted([MarshalAs(UnmanagedType.LPWStr)] string pszFileName);
            void GetCurFile([Out, MarshalAs(UnmanagedType.LPWStr)] string ppszFileName);
        }

        private static void CreateDesktopShortcut(string targetExePath, string shortcutName) {
            try {
                string desktopDir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string lnkPath = Path.Combine(desktopDir, shortcutName + ".lnk");

                // Clean up legacy shortcut if present
                string oldLnk = Path.Combine(desktopDir, "西南民族大学校园网认证.lnk");
                if (File.Exists(oldLnk)) {
                    try { File.Delete(oldLnk); } catch { }
                }

                IShellLinkW link = (IShellLinkW)new ShellLink();
                link.SetPath(targetExePath);
                link.SetWorkingDirectory(Path.GetDirectoryName(targetExePath));
                link.SetIconLocation(targetExePath, 0);
                link.SetDescription("西南民族大学-校园网认证助手 (CampusAuth-SMU)");

                IPersistFile file = (IPersistFile)link;
                file.Save(lnkPath, false);
            } catch {
                // ignore
            }
        }
        #endregion
    }

    static class Program {
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new InstallerForm());
        }
    }
}
