@echo off
chcp 65001 >nul
title 校园网认证助手 - 一键重新编译打包
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0build.ps1"
echo.
pause
