using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace CampusAuth {
    public class AppConfig {
        public string ServerIp { get; set; }
        public string AcId { get; set; }
        public string Username { get; set; }
        public string Domain { get; set; }
        public string Password { get; set; }
        public bool AutoStart { get; set; }
        public bool AutoExitOnSuccess { get; set; }
        public int RetryCount { get; set; }
        public int RetryIntervalSec { get; set; }

        public AppConfig() {
            ServerIp = "211.83.241.102";
            AcId = "1";
            Username = "";
            Domain = "@telecom";
            Password = "";
            AutoStart = false;
            AutoExitOnSuccess = true;
            RetryCount = 10;
            RetryIntervalSec = 3;
        }

        private static string ConfigFilePath {
            get {
                string dir = AppDomain.CurrentDomain.BaseDirectory;
                return Path.Combine(dir, "config.json");
            }
        }

        public static AppConfig Load() {
            var config = new AppConfig();
            try {
                if (File.Exists(ConfigFilePath)) {
                    string json = File.ReadAllText(ConfigFilePath, Encoding.UTF8);
                    config.ServerIp = GetJsonStr(json, "ServerIp", config.ServerIp);
                    config.AcId = GetJsonStr(json, "AcId", config.AcId);
                    config.Username = GetJsonStr(json, "Username", config.Username);
                    config.Domain = GetJsonStr(json, "Domain", config.Domain);
                    string encPwd = GetJsonStr(json, "Password", "");
                    if (!string.IsNullOrEmpty(encPwd)) {
                        config.Password = DecodePassword(encPwd);
                    }
                    config.AutoStart = GetJsonBool(json, "AutoStart", config.AutoStart);
                    config.AutoExitOnSuccess = GetJsonBool(json, "AutoExitOnSuccess", config.AutoExitOnSuccess);
                    config.RetryCount = GetJsonInt(json, "RetryCount", config.RetryCount);
                    config.RetryIntervalSec = GetJsonInt(json, "RetryIntervalSec", config.RetryIntervalSec);
                } else {
                    config.Save();
                }
            } catch {
                // Return default on error
            }
            return config;
        }

        public void Save() {
            try {
                var sb = new StringBuilder();
                sb.AppendLine("{");
                sb.AppendFormat("  \"ServerIp\": \"{0}\",\r\n", EscapeJson(ServerIp));
                sb.AppendFormat("  \"AcId\": \"{0}\",\r\n", EscapeJson(AcId));
                sb.AppendFormat("  \"Username\": \"{0}\",\r\n", EscapeJson(Username));
                sb.AppendFormat("  \"Domain\": \"{0}\",\r\n", EscapeJson(Domain));
                sb.AppendFormat("  \"Password\": \"{0}\",\r\n", EscapeJson(EncodePassword(Password)));
                sb.AppendFormat("  \"AutoStart\": {0},\r\n", AutoStart ? "true" : "false");
                sb.AppendFormat("  \"AutoExitOnSuccess\": {0},\r\n", AutoExitOnSuccess ? "true" : "false");
                sb.AppendFormat("  \"RetryCount\": {0},\r\n", RetryCount);
                sb.AppendFormat("  \"RetryIntervalSec\": {0}\r\n", RetryIntervalSec);
                sb.AppendLine("}");

                File.WriteAllText(ConfigFilePath, sb.ToString(), Encoding.UTF8);
                UpdateAutoStartRegistry(AutoStart);
            } catch {
                // ignore
            }
        }

        public static bool UpdateAutoStartRegistry(bool enable) {
            const string runKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
            const string appName = "CampursAuth-SMU";
            const string oldAppName = "CampusAuth";
            try {
                using (var key = Registry.CurrentUser.OpenSubKey(runKey, true)) {
                    if (key == null) return false;
                    // clean up old key if present
                    if (key.GetValue(oldAppName) != null) {
                        key.DeleteValue(oldAppName, false);
                    }

                    if (enable) {
                        string exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                        string cmd = string.Format("\"{0}\" --silent", exePath);
                        key.SetValue(appName, cmd);
                        return true;
                    } else {
                        if (key.GetValue(appName) != null) {
                            key.DeleteValue(appName, false);
                        }
                        return false;
                    }
                }
            } catch {
                return false;
            }
        }

        public static bool CheckAutoStartRegistry() {
            const string runKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
            const string appName = "CampursAuth-SMU";
            try {
                using (var key = Registry.CurrentUser.OpenSubKey(runKey, false)) {
                    if (key != null) {
                        return key.GetValue(appName) != null;
                    }
                }
            } catch {
                // ignore
            }
            return false;
        }

        private static string EncodePassword(string pwd) {
            if (string.IsNullOrEmpty(pwd)) return "";
            try {
                return Convert.ToBase64String(Encoding.UTF8.GetBytes(pwd));
            } catch {
                return pwd;
            }
        }

        private static string DecodePassword(string enc) {
            if (string.IsNullOrEmpty(enc)) return "";
            try {
                return Encoding.UTF8.GetString(Convert.FromBase64String(enc));
            } catch {
                return enc;
            }
        }

        private static string GetJsonStr(string json, string key, string fallback) {
            var match = Regex.Match(json, "\"" + Regex.Escape(key) + "\"\\s*:\\s*\"([^\"]*)\"");
            return match.Success ? match.Groups[1].Value : fallback;
        }

        private static bool GetJsonBool(string json, string key, bool fallback) {
            var match = Regex.Match(json, "\"" + Regex.Escape(key) + "\"\\s*:\\s*(true|false)", RegexOptions.IgnoreCase);
            return match.Success ? bool.Parse(match.Groups[1].Value) : fallback;
        }

        private static int GetJsonInt(string json, string key, int fallback) {
            var match = Regex.Match(json, "\"" + Regex.Escape(key) + "\"\\s*:\\s*([0-9]+)");
            int val;
            return (match.Success && int.TryParse(match.Groups[1].Value, out val)) ? val : fallback;
        }

        private static string EscapeJson(string s) {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n");
        }
    }
}
