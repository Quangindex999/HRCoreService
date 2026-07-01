# HRCoreService

Dự án này đã được đóng gói bằng Docker để thuận tiện cho việc thiết lập môi trường (bao gồm cả ứng dụng và cơ sở dữ liệu SQL Server). Bạn không cần phải cài đặt SQL Server hay cấu hình thủ công dưới máy cá nhân.

## Yêu cầu hệ thống
- Đã cài đặt **Docker** và **Docker Compose** (Khuyến nghị dùng [Docker Desktop](https://www.docker.com/products/docker-desktop/)).

## Hướng dẫn chạy dự án

1. Clone mã nguồn về máy:
   ```bash
   git clone https://github.com/Quangindex999/HRCoreService.git
   cd HRCoreService
   ```

2. Mở Terminal / PowerShell tại thư mục gốc của dự án (nơi có chứa file `docker-compose.yml`) và chạy lệnh sau để khởi động dự án:
   ```bash
   docker-compose up -d --build
   ```

   **Lệnh này sẽ làm gì?**
   - Tải về Image SQL Server 2022 mới nhất.
   - Build Image cho API `HRCoreService`.
   - Chạy cả 2 container. Khi API khởi động, nó sẽ tự động chạy **Migration** để tạo Database `HRCoreDB` và **Seed** dữ liệu mẫu.

3. Kiểm tra ứng dụng:
   - Mở Swagger UI tại trình duyệt: [http://localhost:8080/swagger](http://localhost:8080/swagger)
   - API đã sẵn sàng để gọi.

## Cách dừng dự án
Để dừng ứng dụng và các container, bạn dùng lệnh:
```bash
docker-compose down
```
*(Nếu muốn xóa cả volume chứa dữ liệu của SQL Server, thêm cờ `-v`: `docker-compose down -v`)*

---
**Lưu ý:** Nếu bạn đang chạy ứng dụng trong Visual Studio / Rider thay vì dùng Docker, hãy đảm bảo bạn có cài SQL Server và sửa lại connection string trong `appsettings.json` cho phù hợp.
