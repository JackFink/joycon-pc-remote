$ErrorActionPreference = "Stop"
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

$targetDir = Join-Path $PSScriptRoot "JoyShockMapper"
$zipPath = Join-Path $PSScriptRoot "JoyShockMapper.zip"

Write-Host "[1/4] JoyShockMapper 최신 릴리스 정보를 확인하는 중..." -ForegroundColor Cyan

$downloadUrl = $null
$releaseTag = $null

try {
    $releases = Invoke-RestMethod -Uri "https://api.github.com/repos/Electronicks/JoyShockMapper/releases" -Headers @{"User-Agent"="PowerShell"}
    foreach ($rel in $releases) {
        foreach ($asset in $rel.assets) {
            if ($asset.name -match "\.zip$") {
                $downloadUrl = $asset.browser_download_url
                $releaseTag = "Electronicks/$($rel.tag_name) ($($asset.name))"
                break
            }
        }
        if ($downloadUrl) { break }
    }
} catch {
    Write-Warning "Electronicks 저장소 조회 실패: $_"
}

if (-not $downloadUrl) {
    throw "JoyShockMapper 다운로드 URL을 찾을 수 없습니다."
}

Write-Host " -> 선택된 버전: $releaseTag" -ForegroundColor Green
Write-Host "[2/4] JoyShockMapper 압축 파일 다운로드 중..." -ForegroundColor Cyan
Invoke-WebRequest -Uri $downloadUrl -OutFile $zipPath -UseBasicParsing

Write-Host "[3/4] 압축 해제 중: $targetDir" -ForegroundColor Cyan
Expand-Archive -Path $zipPath -DestinationPath $targetDir -Force
Remove-Item -Path $zipPath -Force

# 한국어 Windows(CP949)에서 std::filesystem::filesystem_error(0xC0000409)를 유발하는
# 중문 파일명 제거 및 WinMain CommandLineToArgvW(LPSTR) 버그 바이너리 패치
Write-Host "[4/4] 한국어 Windows 호환성 패치 적용 중..." -ForegroundColor Cyan
$jsmSubDir = Join-Path $targetDir "JoyShockMapper_SDL2_x64"
Get-ChildItem -Path $jsmSubDir -Filter "README_*.md" -ErrorAction SilentlyContinue | Remove-Item -Force

$exePath = Join-Path $jsmSubDir "JoyShockMapper.exe"
if (Test-Path $exePath) {
    $bytes = [System.IO.File]::ReadAllBytes($exePath)
    $foff = 0x543AC - 0x1000 + 0x400
    if ($bytes[$foff] -eq 0xFF -and $bytes[$foff+1] -eq 0x15 -and $bytes[$foff+2] -eq 0x1E -and $bytes[$foff+3] -eq 0xD4) {
        $bytes[$foff]   = 0x31
        $bytes[$foff+1] = 0xC0
        $bytes[$foff+2] = 0x90
        $bytes[$foff+3] = 0x90
        $bytes[$foff+4] = 0x90
        $bytes[$foff+5] = 0x90
        [System.IO.File]::WriteAllBytes($exePath, $bytes)
        Write-Host " -> JoyShockMapper.exe WinMain 크래시(0xC0000409) 패치 완료!" -ForegroundColor Green
    }
}

Write-Host "설치 및 패치가 모두 완료되었습니다!" -ForegroundColor Green
