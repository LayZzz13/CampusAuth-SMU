# 西南民族大学-校园网认证助手 自动化编译与打包脚本
$ErrorActionPreference = "Stop"
$dir = $PSScriptRoot
if ([string]::IsNullOrEmpty($dir)) { $dir = (Get-Location).Path }

Write-Host "====================================================" -ForegroundColor Cyan
Write-Host " [1/3] 正在编译 CampursAuth-SMU.exe (主程序)..." -ForegroundColor Yellow
Write-Host "====================================================" -ForegroundColor Cyan

$csc = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
$mainOut = Join-Path $dir "CampursAuth-SMU.exe"
$iconPath = Join-Path $dir "app.ico"
$mainSources = @(
    (Join-Path $dir "SrunClient.cs"),
    (Join-Path $dir "AppConfig.cs"),
    (Join-Path $dir "MainForm.cs"),
    (Join-Path $dir "Program.cs")
)

$mainArgs = @(
    "/target:winexe",
    "/nologo",
    "/out:$mainOut",
    "/r:System.dll",
    "/r:System.Core.dll",
    "/r:System.Drawing.dll",
    "/r:System.Windows.Forms.dll"
)
if (Test-Path $iconPath) {
    $mainArgs += "/win32icon:$iconPath"
    Write-Host " -> 应用图标已挂载: $iconPath" -ForegroundColor Magenta
}
$mainArgs += $mainSources

& $csc $mainArgs
if ($LASTEXITCODE -ne 0) {
    Write-Host "[错误] 主程序编译失败！" -ForegroundColor Red
    exit 1
}
Write-Host " -> 主程序编译成功: $mainOut" -ForegroundColor Green

Write-Host "`n====================================================" -ForegroundColor Cyan
Write-Host " [2/3] 正在自动扫描并打包全部工程文件 (payload.bin)..." -ForegroundColor Yellow
Write-Host "====================================================" -ForegroundColor Cyan

$exclude = @("CampursAuth-SMU-Setup.exe", "payload.bin", "auth.log")
$filesToPack = Get-ChildItem -Path $dir -File | Where-Object { $exclude -notcontains $_.Name }

$payloadPath = Join-Path $dir "payload.bin"
$fs = [System.IO.File]::Create($payloadPath)
$gz = New-Object System.IO.Compression.GZipStream($fs, [System.IO.Compression.CompressionMode]::Compress)
$bw = New-Object System.IO.BinaryWriter($gz, [System.Text.Encoding]::UTF8)

$bw.Write([int]$filesToPack.Count)
foreach ($file in $filesToPack) {
    $bytes = [System.IO.File]::ReadAllBytes($file.FullName)
    $bw.Write([string]$file.Name)
    $bw.Write([int]$bytes.Length)
    $bw.Write($bytes)
    Write-Host "    [已打包] $($file.Name) ($($bytes.Length) 字节)" -ForegroundColor Gray
}
$bw.Flush()
$gz.Close()
$fs.Close()

Write-Host " -> 全部 $($filesToPack.Count) 个文件打包完成: $payloadPath ($((Get-Item $payloadPath).Length) 字节)" -ForegroundColor Green

Write-Host "`n====================================================" -ForegroundColor Cyan
Write-Host " [3/3] 正在编译生成 CampursAuth-SMU-Setup.exe (安装程序)..." -ForegroundColor Yellow
Write-Host "====================================================" -ForegroundColor Cyan

$setupOut = Join-Path $dir "CampursAuth-SMU-Setup.exe"
$installerSource = Join-Path $dir "Installer.cs"

$setupArgs = @(
    "/target:winexe",
    "/nologo",
    "/out:$setupOut",
    "/resource:$payloadPath,payload.bin",
    "/r:System.dll",
    "/r:System.Core.dll",
    "/r:System.Drawing.dll",
    "/r:System.Windows.Forms.dll"
)
if (Test-Path $iconPath) {
    $setupArgs += "/win32icon:$iconPath"
}
$setupArgs += $installerSource

& $csc $setupArgs
$setupSuccess = ($LASTEXITCODE -eq 0)

Remove-Item $payloadPath -Force -ErrorAction SilentlyContinue

if ($setupSuccess) {
    Write-Host " -> 安装包编译成功: $setupOut" -ForegroundColor Green
    Write-Host "`n====================================================" -ForegroundColor Cyan
    Write-Host " [成功] 编译与安全打包全部顺利完成！" -ForegroundColor Green
    Write-Host " 主程序：$mainOut" -ForegroundColor White
    Write-Host " 安装包：$setupOut" -ForegroundColor White
    Write-Host "====================================================" -ForegroundColor Cyan
} else {
    Write-Host "[错误] 安装包编译失败！" -ForegroundColor Red
    exit 1
}
