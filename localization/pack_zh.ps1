# 打包中文版 RHI：发布主程序 + 补齐附属文件
# 用法： powershell -ExecutionPolicy Bypass -File pack_zh.ps1
param(
    [string]$OutDir = ""
)

$ErrorActionPreference = 'Stop'
# 本脚本位于 <仓库根>/localization/ 下，输出目录默认为仓库同级的 RHI-zh-CN
$here   = Split-Path -Parent $MyInvocation.MyCommand.Path
$root   = Split-Path -Parent $here
if (-not $OutDir) { $OutDir = Join-Path (Split-Path -Parent $root) "RHI-zh-CN" }
$src    = Join-Path $root "RenoDXCommander"
$dotnet = if (Test-Path "$env:USERPROFILE\.dotnet\dotnet.exe") { "$env:USERPROFILE\.dotnet\dotnet.exe" } else { "dotnet" }

Write-Host "[1/4] 关闭占用中的进程..."
Get-Process RHI, 'RHI-Stats', 'RHI.DropHelper' -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Seconds 2

Write-Host "[2/4] 发布单文件主程序..."
& $dotnet publish (Join-Path $src "RenoDXCommander.csproj") -c Release -r win-x64 `
    -p:PublishSingleFile=true -p:Platform=x64 --self-contained false -o $OutDir | Out-Null
if ($LASTEXITCODE -ne 0) { throw "发布失败" }

Write-Host "[3/4] 构建拖放助手..."
& $dotnet build (Join-Path $root "RHI.DropHelper\RHI.DropHelper.csproj") -c Release -v quiet | Out-Null
$dh = Join-Path $root "RHI.DropHelper\bin\x64\Release\net8.0-windows"
foreach ($f in @("RHI.DropHelper.exe", "RHI.DropHelper.dll", "RHI.DropHelper.runtimeconfig.json")) {
    $p = Join-Path $dh $f
    if (Test-Path $p) { Copy-Item $p $OutDir -Force }
}

Write-Host "[4/4] 复制附属文件..."
$files = @(
    "icon.ico", "7z.exe", "7z.dll", "ReShade.ini", "ReShade.Vulkan.ini", "ReShade64.json",
    "RHI_PatchNotes.md", "relimiter.ini", "reshade.rdr2.ini",
    "OptiScaler_nightly.nvidia.ini", "OptiScaler_nightly.amd-dlss.ini", "OptiScaler_nightly.amd-nodlss.ini",
    "OptiScaler_dlssnr.nvidia.ini", "OptiScaler_dlssnr.amd-dlss.ini", "OptiScaler_dlssnr.amd-nodlss.ini",
    "OptiScaler_f5.nvidia.ini", "OptiScaler_f5.amd-dlss.ini", "OptiScaler_f5.amd-nodlss.ini",
    "OptiScaler.nvidia.ini", "OptiScaler.amd-dlss.ini", "OptiScaler.amd-nodlss.ini"
)
$missing = @()
foreach ($f in $files) {
    $p = Join-Path $src $f
    if (Test-Path $p) { Copy-Item $p $OutDir -Force } else { $missing += $f }
}

$icons    = Join-Path $src "Assets\icons"
$dstIcons = Join-Path $OutDir "Assets\icons"
New-Item -ItemType Directory -Force -Path $dstIcons | Out-Null
Copy-Item (Join-Path $icons "*.ico") $dstIcons -Force -ErrorAction SilentlyContinue
Copy-Item (Join-Path $icons "*.png") $dstIcons -Force -ErrorAction SilentlyContinue
if (Test-Path (Join-Path $src "Assets\*.png")) {
    Copy-Item (Join-Path $src "Assets\*.png") (Join-Path $OutDir "Assets") -Force -ErrorAction SilentlyContinue
}

if ($missing.Count -gt 0) { Write-Warning ("源码中不存在: " + ($missing -join ', ')) }
Write-Host "打包完成 -> $OutDir"
