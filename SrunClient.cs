using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Collections.Generic;

namespace CampusAuth {
    public class AuthResult {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string OnlineIp { get; set; }
        public string RawResponse { get; set; }
    }

    public class SrunClient {
        public string ServerIp { get; set; }
        public string AcId { get; set; }

        private const string Alpha = "LVoJPiCN2R8G90yg+hmFHuacZ1OWMnrsSTXkYpUq/3dlbfKwv6xztjI7DeBE45QA";
        private const char PadChar = '=';

        public SrunClient(string serverIp = "211.83.241.102", string acId = "1") {
            ServerIp = serverIp;
            AcId = acId;
        }

        public string GetBestLocalIp() {
            try {
                // Priority 1: Physical network adapter with 172.17.x
                foreach (var nic in NetworkInterface.GetAllNetworkInterfaces()) {
                    if (nic.OperationalStatus != OperationalStatus.Up) continue;
                    if (nic.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;

                    string desc = (nic.Description + " " + nic.Name).ToLowerInvariant();
                    if (desc.Contains("meta") || desc.Contains("tunnel") || desc.Contains("tap") ||
                        desc.Contains("vmware") || desc.Contains("virtual") || desc.Contains("plcsim") ||
                        desc.Contains("vethernet") || desc.Contains("bluetooth") || desc.Contains("wsl")) {
                        continue;
                    }

                    var ipProps = nic.GetIPProperties();
                    foreach (var addr in ipProps.UnicastAddresses) {
                        if (addr.Address.AddressFamily == AddressFamily.InterNetwork) {
                            string ipStr = addr.Address.ToString();
                            if (ipStr.StartsWith("172.17.")) {
                                return ipStr;
                            }
                        }
                    }
                }

                // Priority 2: Any physical adapter with 172. or 10.
                foreach (var nic in NetworkInterface.GetAllNetworkInterfaces()) {
                    if (nic.OperationalStatus != OperationalStatus.Up) continue;
                    string desc = (nic.Description + " " + nic.Name).ToLowerInvariant();
                    if (desc.Contains("meta") || desc.Contains("tunnel") || desc.Contains("tap") ||
                        desc.Contains("vmware") || desc.Contains("virtual") || desc.Contains("plcsim")) {
                        continue;
                    }
                    var ipProps = nic.GetIPProperties();
                    foreach (var addr in ipProps.UnicastAddresses) {
                        if (addr.Address.AddressFamily == AddressFamily.InterNetwork) {
                            string ipStr = addr.Address.ToString();
                            if (ipStr.StartsWith("172.") || ipStr.StartsWith("10.")) {
                                return ipStr;
                            }
                        }
                    }
                }
            } catch {
                // fallback
            }

            return "172.17.1.245";
        }

        private string HttpGet(string url, string localIp = null, int timeoutMs = 5000) {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.Timeout = timeoutMs;
            request.ReadWriteTimeout = timeoutMs;
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/153.0.0.0 Safari/537.36 Edg/153.0.0.0";
            request.Referer = string.Format("http://{0}/srun_portal_pc?ac_id={1}&theme=pro", ServerIp, AcId);
            request.Headers.Add("Accept-Language", "zh-CN,zh;q=0.9,en;q=0.8");

            if (!string.IsNullOrEmpty(localIp)) {
                IPAddress bindAddr;
                if (IPAddress.TryParse(localIp, out bindAddr)) {
                    request.ServicePoint.BindIPEndPointDelegate = delegate(ServicePoint servicePoint, IPEndPoint remoteEndPoint, int retryCount) {
                        return new IPEndPoint(bindAddr, 0);
                    };
                }
            }

            using (var response = (HttpWebResponse)request.GetResponse())
            using (var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8)) {
                return reader.ReadToEnd();
            }
        }

        public AuthResult CheckOnlineStatus(string localIp = null) {
            if (string.IsNullOrEmpty(localIp)) {
                localIp = GetBestLocalIp();
            }
            try {
                long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                string cb = "jQuery11240" + now;
                string url = string.Format("http://{0}/cgi-bin/rad_user_info?callback={1}&_={2}", ServerIp, cb, now);
                string res = HttpGet(url, localIp, 3000);

                var match = Regex.Match(res, @"\((.*)\)");
                if (match.Success) {
                    string json = match.Groups[1].Value;
                    string error = ExtractJsonValue(json, "error");
                    string resStatus = ExtractJsonValue(json, "res");
                    string userName = ExtractJsonValue(json, "user_name");
                    string onlineIp = ExtractJsonValue(json, "online_ip");
                    string prod = ExtractJsonValue(json, "products_name");

                    // Crucial: check error != not_online_error and userName must not be empty!
                    if (error != "not_online_error" && resStatus != "not_online_error" && !string.IsNullOrEmpty(userName) && (error == "ok" || resStatus == "ok")) {
                        return new AuthResult {
                            Success = true,
                            OnlineIp = onlineIp,
                            Message = string.Format("已在线 (账号: {0}, 运营商: {1}, IP: {2})", userName, prod, onlineIp),
                            RawResponse = json
                        };
                    } else {
                        return new AuthResult {
                            Success = false,
                            Message = "当前未认证上线 (离线状态)",
                            RawResponse = json
                        };
                    }
                }
                return new AuthResult { Success = false, Message = "响应异常: " + res, RawResponse = res };
            } catch (Exception ex) {
                return new AuthResult { Success = false, Message = "检测在线状态失败: " + ex.Message };
            }
        }

        public AuthResult Login(string rawUsername, string domain, string password, string localIp = null) {
            string fullUsername = rawUsername.Trim() + domain.Trim();
            if (string.IsNullOrEmpty(localIp)) {
                localIp = GetBestLocalIp();
            }

            try {
                // 1. Get Challenge
                long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                string cb1 = "jQuery11240" + now;
                string challengeUrl = string.Format(
                    "http://{0}/cgi-bin/get_challenge?callback={1}&username={2}&ip={3}&_={4}",
                    ServerIp,
                    cb1,
                    Uri.EscapeDataString(fullUsername),
                    Uri.EscapeDataString(localIp),
                    now
                );

                string challengeRes = HttpGet(challengeUrl, localIp, 5000);
                var match = Regex.Match(challengeRes, @"\((.*)\)");
                if (!match.Success) {
                    return new AuthResult { Success = false, Message = "获取 Challenge 失败: " + challengeRes };
                }

                string challengeJson = match.Groups[1].Value;
                string token = ExtractJsonValue(challengeJson, "challenge");
                string clientIp = ExtractJsonValue(challengeJson, "client_ip");
                if (string.IsNullOrEmpty(clientIp)) clientIp = localIp;

                if (string.IsNullOrEmpty(token)) {
                    return new AuthResult { Success = false, Message = "获取 Token 失败: " + challengeJson };
                }

                // 2. Encrypt
                string hmd5 = HmacMd5(token, password);
                string encVer = "srun_bx1";
                string n = "200";
                string type = "1";

                string infoJson = string.Format(
                    "{{\"username\":\"{0}\",\"password\":\"{1}\",\"ip\":\"{2}\",\"acid\":\"{3}\",\"enc_ver\":\"{4}\"}}",
                    EscapeJson(fullUsername),
                    EscapeJson(password),
                    EscapeJson(clientIp),
                    AcId,
                    encVer
                );

                string encInfo = XEncode(infoJson, token);
                string info = "{SRBX1}" + Base64Encode(encInfo);

                string str = token + fullUsername +
                             token + hmd5 +
                             token + AcId +
                             token + clientIp +
                             token + n +
                             token + type +
                             token + info;

                string chksum = Sha1(str);

                // 3. Send Login
                now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                string cb2 = "jQuery11240" + now;

                var queryBuilder = new StringBuilder();
                queryBuilder.AppendFormat("http://{0}/cgi-bin/srun_portal?", ServerIp);
                queryBuilder.AppendFormat("callback={0}", Uri.EscapeDataString(cb2));
                queryBuilder.AppendFormat("&action={0}", "login");
                queryBuilder.AppendFormat("&username={0}", Uri.EscapeDataString(fullUsername));
                queryBuilder.AppendFormat("&password={0}", Uri.EscapeDataString("{MD5}" + hmd5));
                queryBuilder.AppendFormat("&ac_id={0}", Uri.EscapeDataString(AcId));
                queryBuilder.AppendFormat("&ip={0}", Uri.EscapeDataString(clientIp));
                queryBuilder.AppendFormat("&chksum={0}", Uri.EscapeDataString(chksum));
                queryBuilder.AppendFormat("&info={0}", Uri.EscapeDataString(info));
                queryBuilder.AppendFormat("&n={0}", n);
                queryBuilder.AppendFormat("&type={0}", type);
                queryBuilder.AppendFormat("&os={0}", Uri.EscapeDataString("Windows 10"));
                queryBuilder.AppendFormat("&name={0}", Uri.EscapeDataString("windows"));
                queryBuilder.AppendFormat("&double_stack={0}", "0");
                queryBuilder.AppendFormat("&captchaVal=");
                queryBuilder.AppendFormat("&_={0}", now);

                string loginRes = HttpGet(queryBuilder.ToString(), localIp, 6000);
                var matchLogin = Regex.Match(loginRes, @"\((.*)\)");
                if (matchLogin.Success) {
                    string loginJson = matchLogin.Groups[1].Value;
                    string resStatus = ExtractJsonValue(loginJson, "res");
                    string error = ExtractJsonValue(loginJson, "error");
                    string sucMsg = ExtractJsonValue(loginJson, "suc_msg");
                    string errorMsg = ExtractJsonValue(loginJson, "error_msg");
                    string onlineIp = ExtractJsonValue(loginJson, "online_ip");

                    if (error == "ok" || sucMsg == "ip_already_online_error" || resStatus == "ok" || sucMsg == "login_ok") {
                        string desc = (sucMsg == "ip_already_online_error") ? "认证成功 (该设备已在线)" : "登录认证成功！";
                        return new AuthResult {
                            Success = true,
                            OnlineIp = string.IsNullOrEmpty(onlineIp) ? clientIp : onlineIp,
                            Message = string.Format("{0} IP: {1}", desc, string.IsNullOrEmpty(onlineIp) ? clientIp : onlineIp),
                            RawResponse = loginJson
                        };
                    } else {
                        string msg = !string.IsNullOrEmpty(errorMsg) ? errorMsg : (!string.IsNullOrEmpty(error) ? error : loginJson);
                        return new AuthResult { Success = false, Message = "认证失败: " + msg, RawResponse = loginJson };
                    }
                }

                return new AuthResult { Success = false, Message = "登录响应异常: " + loginRes, RawResponse = loginRes };
            } catch (Exception ex) {
                return new AuthResult { Success = false, Message = "认证过程异常: " + ex.Message };
            }
        }

        public AuthResult Logout(string rawUsername, string domain, string localIp = null) {
            string fullUsername = rawUsername.Trim() + domain.Trim();
            if (string.IsNullOrEmpty(localIp)) {
                localIp = GetBestLocalIp();
            }
            try {
                long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                string cb = "jQuery11240" + now;

                // 1. Normal srun_portal logout
                string url1 = string.Format(
                    "http://{0}/cgi-bin/srun_portal?callback={1}&action=logout&username={2}&ip={3}&ac_id={4}&_={5}",
                    ServerIp,
                    cb,
                    Uri.EscapeDataString(fullUsername),
                    Uri.EscapeDataString(localIp),
                    AcId,
                    now
                );
                string res1 = HttpGet(url1, localIp, 4000);

                // 2. DM logout (rad_user_dm for MacAuth / portal)
                long timeSec = now / 1000;
                int unbind = 1;
                string sign = Sha1(timeSec.ToString() + rawUsername.Trim() + localIp + unbind.ToString() + timeSec.ToString());
                string url2 = string.Format(
                    "http://{0}/cgi-bin/rad_user_dm?callback={1}&ip={2}&username={3}&time={4}&unbind={5}&sign={6}&_={7}",
                    ServerIp,
                    cb,
                    Uri.EscapeDataString(localIp),
                    Uri.EscapeDataString(rawUsername.Trim()),
                    timeSec,
                    unbind,
                    sign,
                    now
                );
                string res2 = HttpGet(url2, localIp, 4000);

                return new AuthResult { Success = true, Message = "注销下线成功", RawResponse = res1 + "\n" + res2 };
            } catch (Exception ex) {
                return new AuthResult { Success = false, Message = "注销失败: " + ex.Message };
            }
        }

        #region Crypto Algorithm
        public static string Base64Encode(string s) {
            if (string.IsNullOrEmpty(s)) return s;
            var bytes = new byte[s.Length];
            for (int idx = 0; idx < s.Length; idx++) {
                bytes[idx] = (byte)(s[idx] & 0xFF);
            }
            var sb = new StringBuilder();
            int i = 0;
            int imax = bytes.Length - bytes.Length % 3;
            for (i = 0; i < imax; i += 3) {
                int b10 = (bytes[i] << 16) | (bytes[i + 1] << 8) | bytes[i + 2];
                sb.Append(Alpha[(b10 >> 18) & 63]);
                sb.Append(Alpha[(b10 >> 12) & 63]);
                sb.Append(Alpha[(b10 >> 6) & 63]);
                sb.Append(Alpha[b10 & 63]);
            }
            switch (bytes.Length - imax) {
                case 1:
                    int b1 = bytes[i] << 16;
                    sb.Append(Alpha[(b1 >> 18) & 63]);
                    sb.Append(Alpha[(b1 >> 12) & 63]);
                    sb.Append(PadChar);
                    sb.Append(PadChar);
                    break;
                case 2:
                    int b2 = (bytes[i] << 16) | (bytes[i + 1] << 8);
                    sb.Append(Alpha[(b2 >> 18) & 63]);
                    sb.Append(Alpha[(b2 >> 12) & 63]);
                    sb.Append(Alpha[(b2 >> 6) & 63]);
                    sb.Append(PadChar);
                    break;
            }
            return sb.ToString();
        }

        private static List<uint> S(string a, bool b) {
            int c = a.Length;
            var v = new List<uint>();
            for (int i = 0; i < c; i += 4) {
                uint c0 = (uint)a[i];
                uint c1 = (i + 1 < c) ? (uint)a[i + 1] : 0;
                uint c2 = (i + 2 < c) ? (uint)a[i + 2] : 0;
                uint c3 = (i + 3 < c) ? (uint)a[i + 3] : 0;
                v.Add(c0 | (c1 << 8) | (c2 << 16) | (c3 << 24));
            }
            if (b) {
                v.Add((uint)c);
            }
            return v;
        }

        private static string L(List<uint> a, bool b) {
            int d = a.Count;
            int c = (d - 1) << 2;
            if (b) {
                uint m = a[d - 1];
                if (m < c - 3 || m > c) return null;
                c = (int)m;
            }
            var sb = new StringBuilder();
            for (int i = 0; i < d; i++) {
                sb.Append((char)(a[i] & 0xFF));
                sb.Append((char)((a[i] >> 8) & 0xFF));
                sb.Append((char)((a[i] >> 16) & 0xFF));
                sb.Append((char)((a[i] >> 24) & 0xFF));
            }
            string res = sb.ToString();
            return b ? res.Substring(0, c) : res;
        }

        public static string XEncode(string str, string key) {
            if (string.IsNullOrEmpty(str)) return "";
            var v = S(str, true);
            var k = S(key, false);
            while (k.Count < 4) {
                k.Add(0);
            }
            int n = v.Count - 1;
            uint z = v[n];
            uint y = v[0];
            uint c = 0x86014019 | 0x183639A0;
            uint d = 0;
            int q = 6 + 52 / (n + 1);

            while (0 < q--) {
                d = (d + c) & 0xFFFFFFFF;
                uint e = (d >> 2) & 3;

                for (int p = 0; p < n; p++) {
                    y = v[p + 1];
                    uint m = ((z >> 5) ^ (y << 2)) + (((y >> 3) ^ (z << 4)) ^ (d ^ y)) + (k[(int)((p & 3) ^ e)] ^ z);
                    z = v[p] = (v[p] + m) & 0xFFFFFFFF;
                }

                y = v[0];
                uint m2 = ((z >> 5) ^ (y << 2)) + (((y >> 3) ^ (z << 4)) ^ (d ^ y)) + (k[(int)((n & 3) ^ e)] ^ z);
                z = v[n] = (v[n] + m2) & 0xFFFFFFFF;
            }

            return L(v, false);
        }

        public static string HmacMd5(string key, string message) {
            using (var hmac = new HMACMD5(Encoding.UTF8.GetBytes(key))) {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
                var sb = new StringBuilder();
                for (int i = 0; i < hash.Length; i++) {
                    sb.Append(hash[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }

        public static string Sha1(string message) {
            using (var sha = SHA1.Create()) {
                var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(message));
                var sb = new StringBuilder();
                for (int i = 0; i < hash.Length; i++) {
                    sb.Append(hash[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }
        #endregion

        #region Helpers
        private static string ExtractJsonValue(string json, string key) {
            var match = Regex.Match(json, "\"" + Regex.Escape(key) + "\"\\s*:\\s*\"([^\"]*)\"");
            if (match.Success) return match.Groups[1].Value;
            var matchNum = Regex.Match(json, "\"" + Regex.Escape(key) + "\"\\s*:\\s*([0-9]+)");
            if (matchNum.Success) return matchNum.Groups[1].Value;
            return "";
        }

        private static string EscapeJson(string s) {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n");
        }
        #endregion
    }
}
