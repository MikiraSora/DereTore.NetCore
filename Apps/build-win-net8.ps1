# 设置 baseDir 为 PowerShell 脚本所在的文件夹
$baseDir = Split-Path -Parent $MyInvocation.MyCommand.Definition

# 设置要排除的文件夹
$excludedDirs = @("ScoreViewer","MusicToolchain")

# 设置发布配置文件名（不带 .pubxml 后缀）
$publishProfile = "PublishWindows"

# 设置发布后文件统一复制的目标文件夹
$outputDir =  Get-Location(Join-Path $baseDir "..\bin\publish\win")

# 如果输出文件夹存在则清空，否则创建该文件夹
if (Test-Path $outputDir) {
    Write-Host "Clearing output directory: $outputDir"
    Remove-Item -Recurse -Force -Path $outputDir
}
Write-Host "Creating output directory: $outputDir"
New-Item -ItemType Directory -Path $outputDir | Out-Null

# 遍历 baseDir 中的所有 .csproj 文件
Get-ChildItem -Recurse -Filter *.csproj -Path $baseDir | ForEach-Object {

    $projectPath = $_.FullName
    $projectDir = Split-Path -Parent $projectPath

    # 检查是否在排除的文件夹中
    $shouldSkip = $false
    foreach ($excludedDir in $excludedDirs) {
        if ($projectDir -like "*$excludedDir*") {
            Write-Host "Skipping project: $projectPath (excluded directory)"
            $shouldSkip = $true
            break
        }
    }

    # 如果不需要跳过，执行发布
    if (-not $shouldSkip) {
        Write-Host "-------------------------------------------------------------------"
        Write-Host "Publishing project: $projectPath"
        Write-Host "Using profile: $publishProfile"
        Write-Host "-------------------------------------------------------------------"

        # 为每个项目创建单独的输出文件夹
        $projectOutputDir = Join-Path $outputDir $_.BaseName

        # 执行 dotnet publish 命令
        $publishCommand = "dotnet publish `"$projectPath`" -p:PublishProfile=$publishProfile -f net8.0 -o `"$projectOutputDir`""
        Write-Host "publishCommand: $publishCommand"
        Invoke-Expression $publishCommand

        # 检查发布是否成功
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Error: Failed to publish project $projectPath" -ForegroundColor Red
            exit $LASTEXITCODE
        }
    }
}

Write-Host "-------------------------------------------------------------------"
Write-Host "All projects have been published and outputs copied to $outputDir."
Write-Host "-------------------------------------------------------------------"
