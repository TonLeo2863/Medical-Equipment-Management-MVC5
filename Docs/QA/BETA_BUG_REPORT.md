# Báo cáo lỗi Beta

Các kết quả dưới đây áp dụng cho nhánh `beta-projects`. Build và kiểm thử dùng Windows 10, .NET Framework 4.8, IIS Express và SQL Express 16.0 trên database `MedicalEquipmentDB_BetaTest`.

## BETA-001 · P1 · Seed SQL · FIXED

- **Vị trí** `MedicalEquipmentManagement/Database/Database.sql:240`. Mốc lỗi `6870b41`, bản sửa `d50ee96`.
- **Tái hiện** Chạy bản sao Database.sql đã khóa đích hai lần trên BetaTest. Kỳ vọng lần hai thoát 0 và không đổi dữ liệu.
- **Trước** Lượt hai trả Msg 2627, gồm khóa SA-001 trùng. Câu UPDATE cũ còn có thể trừ kho và ghi đè vị trí của tài sản. [Log trước sửa](Logs/sql-2-Database.sql.log).
- **Nguyên nhân và sửa** INSERT thiếu điều kiện theo khóa nghiệp vụ, tham chiếu dùng ID giả định và các UPDATE seed ghi lại dữ liệu hiện có. Bản sửa tra khóa theo mã, khóa tuần tự phần seed bằng transaction và chèn có điều kiện. Tồn kho mẫu 49 chỉ được đặt khi thêm mới.
- **Sau** Hai lần exit 0, cùng số lượng trên 15 bảng quan sát, tồn kho 49, phòng ban của SA-002 vẫn là 1. Dữ liệu có từ lượt QA trước được giữ lại. [Lượt 1](Logs/seed-retest-1.log) · [Lượt 2](Logs/seed-retest-2.log).
- **Phụ trách** Module kho và database.

## BETA-002 · P1 · Đăng nhập Admin · FIXED sau retest môi trường

- **Vị trí** `MedicalEquipmentManagement/Controllers/AccountController.cs`, action POST Login.
- **Tái hiện** GET Login để lấy cookie và antiforgery token, POST với tài khoản Admin QA có hash PBKDF2, giữ cookie để truy cập Department, rồi logout.
- **Trước** Phiên QA bị gián đoạn sau sự kiện IIS nhận POST. Chưa có bằng chứng lỗi logic xác thực. [Log trước retest](Logs/iis-stdout.log).
- **Sau** HTTP 302, Location `/`, 23.500 ms, `ASP.NET_SessionId` và token cookie có HttpOnly. Session truy cập Department HTTP 200. Logout trả 302 và API sau logout trả 401. Không cần sửa action đăng nhập để xác nhận ca này. [Log HTTP](Logs/http-retest.log).
- **Phụ trách** Module tài khoản và QA.

## BETA-003 · P1 · Web API PUT và DELETE · FIXED

- **Vị trí** `MedicalEquipmentManagement/Web.config:38–46`, bản sửa `d50ee96`.
- **Tái hiện** Admin lấy session và antiforgery token, POST asset demo rồi PUT và DELETE bằng URL extensionless `/api/equipments/{id}`. Hai thao tác hợp lệ phải trả 204.
- **Trước** PUT và DELETE đều trả 404 HTML trong khi GET cùng URL trả 200 JSON. Tài sản vẫn giữ giá cũ và vẫn truy xuất được. [Log trước sửa](Logs/http-retest.log).
- **Nguyên nhân và sửa** Cấu hình IIS chưa khai báo handler ASP.NET cho các verb ghi trên URL extensionless. Khai báo PUT và DELETE và gỡ handler WebDAV trong ứng dụng.
- **Sau** PUT 204, GET kiểm tra giá mới 123, PUT giá âm 400, DELETE 204, GET lại 404. [Log sau sửa](Logs/http-mvc-retest.log).
- **Phụ trách** Web API và cấu hình IIS.

## BETA-004 · P2 · Độ trễ đăng nhập · OPEN

- **Vị trí** `MedicalEquipmentManagement/Controllers/AccountController.cs:24`.
- **Tái hiện** Đăng nhập bằng từng tài khoản QA đã seed hash PBKDF2-SHA256 trên IIS Express. Ghi thời gian POST trước khi theo redirect. Kỳ vọng phản hồi phù hợp với luồng demo tương tác.
- **Thực tế** ADMIN 23.500 ms, MANAGER 17.432 ms, ENGINEER 15.942 ms, STAFF 17.567 ms. Benchmark cùng lớp PBKDF2 của .NET Framework trên máy QA mất 16.692 ms cho một lần dẫn xuất khóa. [Log HTTP](Logs/http-retest.log).
- **Nguyên nhân và hướng sửa** Chi phí dẫn xuất khóa là phần đáng kể của độ trễ. Đo trên máy demo và đánh giá triển khai PBKDF2 tối ưu tương thích định dạng hiện tại. Giữ tham số bảo mật khi đánh giá.
- **Phụ trách** Module tài khoản.
