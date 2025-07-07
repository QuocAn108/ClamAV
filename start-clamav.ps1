# start-clamav.ps1
# Script để khởi động ClamAV Docker container

Write-Host "Starting ClamAV Docker container..." -ForegroundColor Green

# Kiểm tra Docker có đang chạy không
try {
    docker version | Out-Null
} catch {
    Write-Host "Error: Docker is not running. Please start Docker Desktop first." -ForegroundColor Red
    exit 1
}

# Tạo thư mục scan-files nếu chưa tồn tại
if (!(Test-Path "scan-files")) {
    New-Item -ItemType Directory -Path "scan-files"
    Write-Host "Created scan-files directory" -ForegroundColor Yellow
}

# Khởi động ClamAV container
Write-Host "Starting ClamAV container using docker-compose..." -ForegroundColor Yellow
docker-compose up -d

if ($LASTEXITCODE -eq 0) {
    Write-Host "ClamAV container started successfully!" -ForegroundColor Green
    Write-Host "Please wait 2-5 minutes for ClamAV to download virus database..." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "You can check the logs with: docker logs -f clamav-server" -ForegroundColor Cyan
    Write-Host "Test connection with: docker exec clamav-server clamdscan --ping" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Once ready, start your .NET application with: dotnet run" -ForegroundColor Cyan
} else {
    Write-Host "Failed to start ClamAV container!" -ForegroundColor Red
    exit 1
}
