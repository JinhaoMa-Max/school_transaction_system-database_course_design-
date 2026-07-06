<#=============================================================================
  校园二手交易系统 - Windows Docker 环境一键配置脚本
  以管理员身份运行：右键 → 使用 PowerShell 运行
=============================================================================#>

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Docker 环境配置脚本" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# ---- 1. 确保 Docker Desktop 已安装 ----
$dockerPath = "$env:ProgramFiles\Docker\Docker\Docker Desktop.exe"
if (-not (Test-Path $dockerPath)) {
    Write-Host "❌ 未检测到 Docker Desktop，请先下载安装：" -ForegroundColor Red
    Write-Host "   https://desktop.docker.com/win/main/amd64/Docker%20Desktop%20Installer.exe" -ForegroundColor Yellow
    Write-Host "   安装完成后再运行此脚本"
    Read-Host "按回车退出"
    exit 1
}
Write-Host "✅ Docker Desktop 已安装" -ForegroundColor Green

# ---- 2. 确保 D 盘存在 ----
if (-not (Test-Path "D:\")) {
    Write-Host "❌ 未检测到 D 盘，将使用 C 盘存储数据" -ForegroundColor Yellow
    $targetDir = "$env:USERPROFILE\docker-data"
} else {
    $targetDir = "D:\docker-data"
}
Write-Host "→ 数据目录: $targetDir" -ForegroundColor Cyan

# ---- 3. 创建 Docker 数据目录 ----
if (-not (Test-Path $targetDir)) {
    New-Item -ItemType Directory -Path $targetDir -Force | Out-Null
    Write-Host "✅ 已创建数据目录: $targetDir" -ForegroundColor Green
}

# ---- 4. 创建 Docker 配置文件（将数据迁移到 D 盘）----
$configDir = "$env:AppData\Docker"
if (-not (Test-Path $configDir)) {
    New-Item -ItemType Directory -Path $configDir -Force | Out-Null
}

$daemonConfig = @"
{
    "data-root": "$($targetDir -replace '\\', '/')",
    "wslEngineEnabled": true,
    "features": {
        "buildkit": true
    }
}
"@

$daemonPath = "$configDir\settings.json"
# 检查是否存在，如果存在则备份
if (Test-Path $daemonPath) {
    $backup = "$daemonPath.backup_$(Get-Date -Format 'yyyyMMddHHmmss')"
    Copy-Item $daemonPath $backup
    Write-Host "📦 已备份原有配置到: $backup" -ForegroundColor Yellow
}

Set-Content -Path $daemonPath -Value $daemonConfig -Encoding UTF8
Write-Host "✅ 已配置 Docker 数据目录到: $targetDir" -ForegroundColor Green

# ---- 5. 启动 Docker Desktop ----
Write-Host ""
Write-Host "正在启动 Docker Desktop..." -ForegroundColor Cyan
Start-Process "$env:ProgramFiles\Docker\Docker\Docker Desktop.exe"

Write-Host "⏳ 等待 Docker 启动（约30秒）..." -ForegroundColor Yellow
Start-Sleep -Seconds 30

# ---- 6. 验证 Docker 是否运行 ----
$retry = 0
do {
    $retry++
    Start-Sleep -Seconds 3
    $dockerOk = docker info 2>$null
} while (-not $dockerOk -and $retry -lt 15)

if ($dockerOk) {
    Write-Host "✅ Docker 已成功运行！" -ForegroundColor Green
    docker --version
} else {
    Write-Host "⚠️ Docker 启动中，请手动打开 Docker Desktop 检查状态" -ForegroundColor Yellow
}

# ---- 7. 验证 WSL2 集成 ----
Write-Host ""
Write-Host "正在检查 WSL2 集成..." -ForegroundColor Cyan
$wslCheck = wsl -l -v 2>$null
if ($wslCheck -match "Ubuntu.*2") {
    Write-Host "✅ WSL2 Ubuntu 已就绪" -ForegroundColor Green
} else {
    Write-Host "⚠️ WSL2 状态异常，请检查" -ForegroundColor Yellow
}

# ---- 8. 配置 WSL 内存限制（防止 Oracle 占满内存）----
$wslConfigPath = "$env:USERPROFILE\.wslconfig"
if (-not (Test-Path $wslConfigPath)) {
    $wslConfig = @"
[wsl2]
memory=4GB
processors=4
localhostForwarding=true
"@
    Set-Content -Path $wslConfigPath -Value $wslConfig -Encoding UTF8
    Write-Host "✅ 已配置 WSL2 内存限制为 4GB" -ForegroundColor Green
    Write-Host "⚠️ 需要重启 WSL 生效，将在脚本结束时执行" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  环境配置完成！" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "接下来运行 Oracle 容器：" -ForegroundColor Cyan
Write-Host "  cd C:\Users\aaa\Desktop\homework\database\school_transaction_system-database_course_design-\scripts" -ForegroundColor White
Write-Host "  docker compose up -d" -ForegroundColor White
Write-Host ""
Write-Host "查看日志：" -ForegroundColor Cyan
Write-Host "  docker compose logs -f" -ForegroundColor White
Write-Host ""

# ---- 9. 重启 WSL 使配置生效 ----
$restart = Read-Host "是否现在重启 WSL？（需要确认，y/n）"
if ($restart -eq 'y') {
    Write-Host "正在重启 WSL..." -ForegroundColor Cyan
    wsl --shutdown
    Start-Sleep -Seconds 3
    Write-Host "✅ WSL 已重启" -ForegroundColor Green
}

Read-Host "按回车退出"
