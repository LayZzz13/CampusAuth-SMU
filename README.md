<div align="center">

<img src="icon.png" width="128" height="128" alt="CampusAuth-SMU Logo" />

# 西南民族大学 - 校园网认证助手 (CampusAuth-SMU)

**轻量 · 静默 · 安全 · 开箱即用的西南民族大学深澜 (SRun 4000) 校园网自动认证工具**

[![Release](https://img.shields.io/badge/Release-v1.0.2-blue.svg)](https://github.com/LayZzz13/CampusAuth-SMU/releases)
[![Platform](https://img.shields.io/badge/Platform-Windows-0078D6.svg)](https://www.microsoft.com/windows)
[![Framework](https://img.shields.io/badge/.NET_Framework-4.0%2B-512BD4.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Author](https://img.shields.io/badge/Author-小刺猬-orange.svg)](https://link3.cc/zscc5401)

</div>

---

## 📖 项目简介

**CampusAuth-SMU** 是专为**西南民族大学（SMU）**师生定制开发的校园网快捷认证工具。针对校内深澜（SRun 4000）网页认证网关（`211.83.241.102`），本程序解决了频繁手动打开浏览器、重定向跳转缓慢、断网重连繁琐等痛点，提供**开机后台静默自动联网**与**精美可视化控制面板**双重使用体验。

---

## ✨ 核心特性

- 🚀 **开机后台静默自动认证**：
  - 开机自动在后台完成网络检测与认证，不弹窗、不闪黑框。
  - 认证成功后**自动退出程序**，绝日常驻后台占用系统内存。
- 🖥️ **精美可视化控制面板**：
  - 提供【一键登录 / 认证】、【注销 / 下线】、【状态检测】、【保存设置】。
  - 实时检测当前校园网在线状态、登录账号、运营商与本机物理内网 IP。
  - 支持密码明密文快速切换显示。
- 🌐 **智能网卡与虚拟网卡穿透**：
  - 自研物理网卡识别算法，自动绕过 Meta Tunnel、Clash、TUN 等虚拟网卡，精准绑定物理以太网或 Wi-Fi 适配器（如 `172.17.x.x`）。
- 🔒 **严格的隐私与安全保护**：
  - 出厂配置脱敏，不内置任何个人账号与密码。
  - 密码采用本地编码存储于用户本机的 `config.json` 中，**绝不上报任何第三方服务器**。
  - 纯标准 HTTP 请求直连校内官方网关（`211.83.241.102`）。
- 🛡️ **纯净安全，无杀软误报**：
  - 彻底移除了脚本宿主反射与直接注入注册表的高危特征，采用 Windows 原生 Shell COM 接口创建快捷方式。
  - 兼容腾讯电脑管家、360 安全卫士、Windows Defender 等杀毒软件实时防护。
- 📦 **开箱即用，零第三方依赖**：
  - 基于 Windows 原生 .NET Framework 构建，无需安装 Python、Node.js 等额外运行环境。
  - 提供一键安装包向导与绿色免安装双重模式。

---

## 📸 界面预览

- **桌面快捷方式**：`CampusAuth-SMU`（内置专属定制图标）
- **程序主界面**：
  - 顶部版本与作者信息：`西南民族大学-校园网认证助手 v1.0.2 @小刺猬`
  - 最新版本更新提醒横幅（可自由关闭）
  - 账号、密码、运营商选择（中国电信 `@telecom` / 中国移动 `@cmccgx-test` / 中国联通 `@cucc` / 校园网 `@swun`）
  - 独立醒目的【开机自启动】权限一键绑定与切换按钮
  - 实时网络状态指示灯与详细操作日志终端

---

## 🚀 快速上手

### 方式一：使用安装向导（推荐）
1. 在 [Releases](https://github.com/LayZzz13/CampusAuth-SMU/releases) 页面下载最新的 `CampursAuth-SMU-Setup.exe`。
2. 双击运行安装向导，阅读并同意许可协议。
3. 选择安装目录（默认安装至用户应用目录，无需管理员权限）。
4. 点击【开始安装】，安装完成后会自动在桌面生成 `CampusAuth-SMU` 快捷方式。
5. 首次打开程序，输入您的学工号、密码并选择运营商，点击【保存设置】或【一键登录 / 认证】。
6. 点击界面上的【开机自启动】按钮，即可一键享受开机免打扰自动联网！

### 方式二：绿色免安装
1. 直接下载 `CampursAuth-SMU.exe` 和 `config.json` 到任意文件夹。
2. 双击 `CampursAuth-SMU.exe` 即可直接使用。

---

## 🛠️ 从源码编译

本项目采用纯原生 Windows C# 编写，无需配置 Visual Studio，仅需使用 Windows 自带的 .NET Framework 编译器（`csc.exe`）即可一键编译与打包。

### 环境要求
- Windows 7 / 8 / 10 / 11
- .NET Framework 4.0 或更高版本（Windows 系统均自带）

### 一键编译命令
直接在项目根目录下双击运行：
```cmd
一键重新编译.bat
```
或者在 PowerShell 中执行：
```powershell
.\build.ps1
```

编译脚本将自动执行以下三个阶段：
1. 调用 `csc.exe` 编译主程序 `CampursAuth-SMU.exe`（自动注入 `app.ico` 图标与版本属性）；
2. 扫描并打包所有工程文件生成高压缩比的 `payload.bin` 数据包；
3. 编译生成独立的单文件安装向导 `CampursAuth-SMU-Setup.exe`。

---

## 📂 项目文件结构

```text
CampusAuth-SMU/
├── app.ico                  # 软件主图标 (包含 256x256 至 16x16 全套分辨率)
├── icon.png                 # 高清项目 Logo 预览图
├── AppConfig.cs             # 本地配置读写、密码编码与注册表自启动管理
├── SrunClient.cs            # 深澜 SRun 4000 协议核心通信与加密算法实现
├── MainForm.cs              # WinForms 可视化管理面板与事件交互
├── Program.cs               # 程序主入口与后台静默认证调度器 (--silent)
├── Installer.cs             # 独立安装向导程序源码 (基于原生 COM 快捷方式)
├── config.json              # 默认脱敏配置文件模板
├── build.ps1                # PowerShell 自动化编译与资源打包核心脚本
├── 一键重新编译.bat          # 批处理一键编译快捷入口
├── 使用说明.txt              # 离线使用文档与作者联系方式
├── .gitignore               # Git 忽略规则
├── LICENSE                  # MIT 开源许可证
└── README.md                # 项目详细说明文档
```

---

## 🔬 深澜 SRun 4000 逆向协议解析

本项目完全独立实现了深澜 SRun 4000 认证协议：
1. **Challenge 令牌获取**：
   - 请求 `/cgi-bin/get_challenge` 获取动态加密 Token 与客户端 IP。
2. **密码哈希计算**：
   - 采用 `HMAC-MD5(token, password)` 计算密码认证串。
3. **UserInfo 结构与 XXTEA 加密**：
   - 将用户名、密码、IP、AC_ID 等结构序列化为 JSON；
   - 采用深澜定制的变种 XXTEA (`srun_bx1`) 算法加密，配合特定的 Base64 字典（`LVoJPiCN...`）编码生成 `info` 载荷。
4. **校验和 Checksum**：
   - 拼接 Token、用户名、HMD5、时间戳及加密串，计算 `SHA1` 散列值作为校验签名。
5. **双通道注销下线**：
   - 结合 `/cgi-bin/srun_portal?action=logout` 与 `/cgi-bin/rad_user_dm`，确保在不同网关接入模式下均能顺畅下线。

---

## ⚠️ 免责声明

1. 本项目仅供西南民族大学师生学习交流与便捷网络接入使用，请勿用于任何违反校规校纪或破坏校园网络安全的行为。
2. 使用本软件时须自觉遵守《中华人民共和国网络安全法》及《西南民族大学校园网络安全管理规定》。
3. 因用户个人账号欠费、停机、密码泄露或官方认证系统变动等引起的网络故障，开发者不承担连带责任。

---

## 👨‍💻 作者与支持

- **作者**：小刺猬
- **微信联系**：`z2531411694l`
- **官方更新地址**：[https://link3.cc/zscc5401](https://link3.cc/zscc5401)

如果您在使用过程中遇到任何 Bug 或有改进建议，欢迎提交 [Issues](https://github.com/LayZzz13/CampusAuth-SMU/issues) 或添加作者微信交流！欢迎 Star ⭐️ 支持本项目！
