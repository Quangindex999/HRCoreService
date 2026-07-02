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

## Cách 2: Chạy trực tiếp (Không dùng Docker - Dành cho DEV)

Nếu bạn không muốn dùng Docker mà muốn chạy trực tiếp bằng Visual Studio, Rider hoặc lệnh `dotnet run`, hãy làm theo các bước sau:

1. **Cập nhật Connection String**:
   Mở file `HRCoreService/appsettings.json`, tìm đến mục `ConnectionStrings:HRCoreDB` và sửa thông tin `Server` thành tên SQL Server trên máy bạn (ví dụ: `.\SQLEXPRESS`, `localhost`, v.v.).

2. **Chạy dự án**:
   Mở Terminal tại thư mục `HRCoreService` (chứa file `.csproj`) và gõ:
   ```bash
   dotnet run
   ```
   Hoặc đơn giản là bấm nút Play (Run) trong Visual Studio.

3. **Database tự động sinh ra**:
   Code đã được cấu hình tính năng Auto-Migration. Ngay khi app vừa chạy lên, nó sẽ tự động kết nối vào SQL Server của bạn, tạo Database `HRCoreDB` với đầy đủ các bảng và dữ liệu mẫu (admin, hr...). 
   *(Bạn không cần phải chạy file .bak hay tạo tay bất cứ thứ gì!)*

> **Lưu ý quan trọng cho team:** Các lỗi cũ về Migration (như lỗi không tìm thấy bảng) đã được fix. Hãy chắc chắn bạn đã pull code mới nhất (bao gồm cả thư mục `Migrations` mới) trước khi chạy.
