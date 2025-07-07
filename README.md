# TestClamAV - ClamAV Antivirus Integration với ASP.NET Core

## Giới thiệu

TestClamAV là một dự án ASP.NET Core Web API tích hợp với ClamAV (Clam AntiVirus) để cung cấp khả năng quét virus cho các file và nội dung. Dự án sử dụng Docker để triển khai ClamAV server một cách dễ dàng và hiệu quả.

## ClamAV là gì?

ClamAV là một engine antivirus mã nguồn mở được thiết kế để phát hiện trojan, virus, malware và các mối đe dọa độc hại khác. Nó được sử dụng rộng rãi trong các hệ thống email server, web gateway và các ứng dụng cần khả năng quét virus.

### Tính năng chính của ClamAV:

- **Mã nguồn mở**: Hoàn toàn miễn phí và có thể tùy chỉnh
- **Cập nhật định kỳ**: Database virus được cập nhật thường xuyên
- **Hiệu suất cao**: Tối ưu cho việc quét file lớn
- **Đa nền tảng**: Hỗ trợ Windows, Linux, macOS
- **API linh hoạt**: Cung cấp daemon (clamd) để tích hợp với các ứng dụng

## Kiến trúc dự án

```
TestClamAV/
├── Controllers/
│   └── ScanController.cs      # API endpoints cho việc quét virus
├── Services/
│   └── ClamAVService.cs       # Service xử lý logic quét virus
├── Program.cs                 # Entry point của ứng dụng
├── appsettings.json          # Cấu hình ứng dụng
└── TestClamAV.csproj         # File project
```

## Cài đặt và sử dụng ClamAV với Docker

### 1. Cài đặt ClamAV sử dụng Docker

#### Option 1: Sử dụng Docker Compose (Khuyên dùng)

Tạo file `docker-compose.yml`:

```yaml
version: "3.8"
services:
  clamav:
    image: clamav/clamav:latest
    container_name: clamav-server
    ports:
      - "3310:3310"
    volumes:
      - clamav-data:/var/lib/clamav
      - ./scan-files:/scan # Mount thư mục chứa file cần quét
    environment:
      - CLAMAV_NO_FRESHCLAM=false # Tự động cập nhật virus database
    restart: unless-stopped
    healthcheck:
      test: ["CMD", "clamdscan", "--ping", "1"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 120s

volumes:
  clamav-data:
```

Chạy container:

```bash
docker-compose up -d
```

#### Option 2: Sử dụng Docker run trực tiếp

```bash
# Pull image ClamAV mới nhất
docker pull clamav/clamav:latest

# Chạy ClamAV container
docker run -d \
  --name clamav-server \
  -p 3310:3310 \
  -v clamav-data:/var/lib/clamav \
  -v ./scan-files:/scan \
  clamav/clamav:latest
```

### 2. Kiểm tra trạng thái ClamAV

```bash
# Kiểm tra container đang chạy
docker ps | grep clamav

# Xem logs của ClamAV
docker logs clamav-server

# Kiểm tra ClamAV daemon
docker exec clamav-server clamdscan --ping
```

### 3. Cấu hình ứng dụng .NET

Cập nhật `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ClamAV": {
    "Host": "127.0.0.1",
    "Port": 3310,
    "MaxStreamSize": 52428800
  }
}
```

### 4. Chạy ứng dụng .NET

```bash
# Build và chạy project
dotnet build
dotnet run

# Hoặc sử dụng Visual Studio
# Nhấn F5 hoặc Ctrl+F5
```

## API Endpoints

Ứng dụng cung cấp các API endpoints sau:

### 1. Ping ClamAV Server

```http
GET /api/scan/ping
```

Kiểm tra trạng thái kết nối với ClamAV server.

### 2. Quét file theo đường dẫn

```http
POST /api/scan/scan-file
Content-Type: application/x-www-form-urlencoded

filePath=C:\path\to\your\file.txt
```

### 3. Quét nội dung HTML

```http
POST /api/scan/scan-html
Content-Type: application/x-www-form-urlencoded

htmlContent=<html><body>Your HTML content here</body></html>
```

### 4. Quét file upload (PDF và các file khác)

```http
POST /api/scan/scan-pdf
Content-Type: multipart/form-data

file: [your_file]
```

## Ví dụ sử dụng

### Sử dụng cURL

```bash
# Ping ClamAV
curl -X GET "https://localhost:7000/api/scan/ping"

# Quét file
curl -X POST "https://localhost:7000/api/scan/scan-file" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "filePath=C:\temp\test.txt"

# Quét HTML content
curl -X POST "https://localhost:7000/api/scan/scan-html" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "htmlContent=<html><body>Hello World</body></html>"

# Upload và quét file
curl -X POST "https://localhost:7000/api/scan/scan-pdf" \
  -H "Content-Type: multipart/form-data" \
  -F "file=@test.pdf"
```

### Sử dụng PowerShell

```powershell
# Ping ClamAV
Invoke-RestMethod -Uri "https://localhost:7000/api/scan/ping" -Method GET

# Quét file
$body = @{ filePath = "C:\temp\test.txt" }
Invoke-RestMethod -Uri "https://localhost:7000/api/scan/scan-file" -Method POST -Body $body

# Upload và quét file
$filePath = "C:\temp\test.pdf"
$form = @{
    file = Get-Item -Path $filePath
}
Invoke-RestMethod -Uri "https://localhost:7000/api/scan/scan-pdf" -Method POST -Form $form
```

## Troubleshooting

### Các vấn đề thường gặp

1. **ClamAV container khởi động chậm**

   - ClamAV cần thời gian để tải và cập nhật virus database
   - Đợi khoảng 2-5 phút sau khi start container
   - Kiểm tra logs: `docker logs clamav-server`

2. **Kết nối bị từ chối (Connection refused)**

   ```bash
   # Kiểm tra port 3310 có mở không
   netstat -an | findstr 3310

   # Kiểm tra firewall
   # Đảm bảo port 3310 không bị block
   ```

3. **File không tìm thấy khi quét**

   - Đảm bảo file path đúng và file tồn tại
   - Với Docker, mount volume chứa file cần quét
   - Sử dụng endpoint upload file thay vì file path

4. **Virus database lỗi thời**

   ```bash
   # Cập nhật manual virus database
   docker exec clamav-server freshclam

   # Restart container để áp dụng update
   docker restart clamav-server
   ```

### Logs và Monitoring

```bash
# Theo dõi logs realtime
docker logs -f clamav-server

# Kiểm tra version ClamAV
docker exec clamav-server clamdscan --version

# Kiểm tra virus database
docker exec clamav-server sigtool --info /var/lib/clamav/main.cvd
```

## Bảo mật và Production

### Khuyến nghị cho môi trường Production:

1. **Network Security**:

   - Chỉ expose port 3310 cho các service cần thiết
   - Sử dụng internal Docker network
   - Cấu hình firewall appropriately

2. **Resource Limits**:

   ```yaml
   clamav:
     image: clamav/clamav:latest
     deploy:
       resources:
         limits:
           memory: 2G
           cpus: "1.0"
         reservations:
           memory: 1G
   ```

3. **Monitoring**:

   - Thiết lập health check
   - Monitor memory usage (ClamAV có thể consume nhiều RAM)
   - Thiết lập alerts cho virus detection

4. **Backup và Recovery**:
   - Backup volume chứa virus database
   - Thiết lập script tự động update database

## Dependencies

- **.NET 8.0**: Framework chính
- **nClam 9.0.0**: .NET client library cho ClamAV
- **Swashbuckle.AspNetCore**: Swagger/OpenAPI documentation
- **Docker**: Container platform cho ClamAV

## Tài liệu tham khảo

- [ClamAV Official Documentation](https://docs.clamav.net/)
- [ClamAV Docker Hub](https://hub.docker.com/r/clamav/clamav)
- [nClam Library](https://github.com/tekmaven/nClam)
- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)

## License

Dự án này được phát hành dưới MIT License. ClamAV sử dụng GPL License.
