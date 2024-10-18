# ���� baseDir Ϊ PowerShell �ű����ڵ��ļ���
$baseDir = Split-Path -Parent $MyInvocation.MyCommand.Definition

# ����Ҫ�ų����ļ���
$excludedDirs = @("ScoreViewer","MusicToolchain")

# ���÷��������ļ��������� .pubxml ��׺��
$publishProfile = "PublishWindows"

# ���÷������ļ�ͳһ���Ƶ�Ŀ���ļ���
$outputDir =  Get-Location(Join-Path $baseDir "..\bin\publish\win")

# �������ļ��д�������գ����򴴽����ļ���
if (Test-Path $outputDir) {
    Write-Host "Clearing output directory: $outputDir"
    Remove-Item -Recurse -Force -Path $outputDir
}
Write-Host "Creating output directory: $outputDir"
New-Item -ItemType Directory -Path $outputDir | Out-Null

# ���� baseDir �е����� .csproj �ļ�
Get-ChildItem -Recurse -Filter *.csproj -Path $baseDir | ForEach-Object {

    $projectPath = $_.FullName
    $projectDir = Split-Path -Parent $projectPath

    # ����Ƿ����ų����ļ�����
    $shouldSkip = $false
    foreach ($excludedDir in $excludedDirs) {
        if ($projectDir -like "*$excludedDir*") {
            Write-Host "Skipping project: $projectPath (excluded directory)"
            $shouldSkip = $true
            break
        }
    }

    # �������Ҫ������ִ�з���
    if (-not $shouldSkip) {
        Write-Host "-------------------------------------------------------------------"
        Write-Host "Publishing project: $projectPath"
        Write-Host "Using profile: $publishProfile"
        Write-Host "-------------------------------------------------------------------"

        # Ϊÿ����Ŀ��������������ļ���
        $projectOutputDir = Join-Path $outputDir $_.BaseName

        # ִ�� dotnet publish ����
        $publishCommand = "dotnet publish `"$projectPath`" -p:PublishProfile=$publishProfile -f net8.0 -o `"$projectOutputDir`""
        Write-Host "publishCommand: $publishCommand"
        Invoke-Expression $publishCommand

        # ��鷢���Ƿ�ɹ�
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Error: Failed to publish project $projectPath" -ForegroundColor Red
            exit $LASTEXITCODE
        }
    }
}

Write-Host "-------------------------------------------------------------------"
Write-Host "All projects have been published and outputs copied to $outputDir."
Write-Host "-------------------------------------------------------------------"
