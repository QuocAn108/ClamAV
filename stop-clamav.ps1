# stop-clamav.ps1
# Script để dừng ClamAV Docker container

Write-Host "Stopping ClamAV Docker container..." -ForegroundColor Yellow

# Dừng container
docker-compose down

if ($LASTEXITCODE -eq 0) {
    Write-Host "ClamAV container stopped successfully!" -ForegroundColor Green
} else {
    Write-Host "Failed to stop ClamAV container!" -ForegroundColor Red
    exit 1
}

# Hỏi có muốn xóa volumes không
$removeVolumes = Read-Host "Do you want to remove ClamAV data volumes? (y/N)"
if ($removeVolumes -eq "y" -or $removeVolumes -eq "Y") {
    docker-compose down -v
    Write-Host "ClamAV data volumes removed!" -ForegroundColor Yellow
}
