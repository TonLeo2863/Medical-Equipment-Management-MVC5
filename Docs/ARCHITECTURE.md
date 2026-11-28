# Kiến trúc ứng dụng

```mermaid
flowchart LR
    Browser["Trình duyệt và Razor"] --> MVC["Controller ASP.NET MVC 5"]
    Client["Postman hoặc ứng dụng tích hợp"] --> Api["Web API 2"]
    MVC --> Access["SessionAccount và bộ lọc vai trò"]
    Api --> Access
    MVC --> Rules["ViewModel và quy tắc nghiệp vụ"]
    Api --> Rules
    Rules --> EF["EF6 DbContext và LINQ"]
    Access --> EF
    EF --> DB["SQL Server"]
```

`Views/Shared/_Layout.cshtml` sở hữu Sidebar, thông báo TempData và tài nguyên CSS/JS chung. Module giữ ModelState và điều hướng nghiệp vụ. `AccountInputs` giới hạn binding. `PasswordHasher` xử lý dẫn xuất khóa và xác minh. `SessionAccount` đối chiếu hash và trạng thái quyền cho mỗi phiên.

`EquipmentPolicy` chia sẻ điều kiện thay đổi tài sản giữa MVC và Web API. `MedicalEquipmentContext.LockAsset` bảo vệ các workflow cùng sửa vị trí hoặc trạng thái. Dashboard dùng LINQ và Chart.js cục bộ. [Hướng dẫn đọc mã](SOURCE_CODE_GUIDE.md)
