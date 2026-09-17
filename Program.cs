using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("西南民族大学-校园网认证助手")]
[assembly: AssemblyDescription("西南民族大学深澜校园网自动认证工具")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("小刺猬")]
[assembly: AssemblyProduct("西南民族大学-校园网认证助手")]
[assembly: AssemblyCopyright("Copyright © 2026 小刺猬. All Rights Reserved.")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: AssemblyVersion("1.0.2.0")]
[assembly: AssemblyFileVersion("1.0.2.0")]

namespace CampusAuth {
    static class Program {
        [STAThread]
        static void Main(string[] args) {
            bool isSilent = false;
            foreach (var arg in args) {
                if (arg.Equals("--silent", StringComparison.OrdinalIgnoreCase) ||
                    arg.Equals("--auto", StringComparison.OrdinalIgnoreCase) ||
                    arg.Equals("-s", StringComparison.OrdinalIgnoreCase)) {
                    isSilent = true;
                    break;
                }
            }

            if (isSilent) {
                RunSilentMode();
            } else {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
            }
        }

        private static void RunSilentMode() {
            var config = AppConfig.Load();
            var client = new SrunClient(config.ServerIp, config.AcId);

            LogSilent("=== 开机后台静默登录启动 ===");
            if (string.IsNullOrEmpty(config.Username) || string.IsNullOrEmpty(config.Password)) {
                LogSilent("提示：账号或密码为空，跳过后台静默登录。请先打开主程序输入账号密码并保存。");
                LogSilent("=== 退出程序 ===");
                return;
            }
            LogSilent(string.Format("目标服务器: {0}, 账号: {1}{2}", config.ServerIp, config.Username, config.Domain));

            int maxRetries = Math.Max(config.RetryCount, 5);
            int intervalMs = Math.Max(config.RetryIntervalSec, 2) * 1000;

            for (int attempt = 1; attempt <= maxRetries; attempt++) {
                try {
                    LogSilent(string.Format("第 {0}/{1} 次尝试检测与登录...", attempt, maxRetries));

                    // 1. Check if already online
                    var status = client.CheckOnlineStatus();
                    if (status.Success) {
                        LogSilent("网络检测已处于在线状态: " + status.Message);
                        LogSilent("=== 任务完成，静默退出 ===");
                        return;
                    }

                    // 2. Perform Login
                    var result = client.Login(config.Username, config.Domain, config.Password);
                    if (result.Success) {
                        LogSilent("认证成功: " + result.Message);
                        LogSilent("=== 任务完成，静默退出 ===");
                        return;
                    } else {
                        LogSilent("认证未成功: " + result.Message);
                    }
                } catch (Exception ex) {
                    LogSilent(string.Format("尝试发生异常: {0}", ex.Message));
                }

                if (attempt < maxRetries) {
                    Thread.Sleep(intervalMs);
                }
            }

            LogSilent("=== 达到最大重试次数，退出程序 ===");
        }

        private static void LogSilent(string message) {
            try {
                string dir = AppDomain.CurrentDomain.BaseDirectory;
                string logFile = Path.Combine(dir, "auth.log");
                string logLine = string.Format("[{0}] {1}\r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), message);
                File.AppendAllText(logFile, logLine, Encoding.UTF8);
            } catch {
                // ignore
            }
        }
    }
}
