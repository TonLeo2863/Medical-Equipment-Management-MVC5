# Quản lý trang thiết bị y tế

Ứng dụng đồ án môn Lập trình Web theo dõi từng thiết bị vật lý từ nhập kho, phân bổ, điều chuyển, bảo trì, hiệu chuẩn, sửa chữa đến thanh lý. Nền tảng gồm ASP.NET MVC 5, .NET Framework 4.8, Entity Framework 6, Razor, LINQ, SQL Server và ASP.NET Web API 2.

## Chạy trên Windows

1. Dùng Visual Studio với gói phát triển ASP.NET và .NET Framework 4.8 Developer Pack. Cài IIS Express và SQL Server Express.
2. Mở `MedicalEquipmentManagement.sln` và Restore NuGet theo `packages.config`. Có thể chạy `MSBuild MedicalEquipmentManagement.sln /t:Restore /p:RestorePackagesConfig=true`.
3. Dùng database QA đã chuẩn bị `MedicalEquipmentDB_BetaTest` trên `localhost\SQLEXPRESS`. Kiểm tra `MedicalEquipmentConnection` trong `MedicalEquipmentManagement/Web.config`; cấu hình nhánh dùng Windows Authentication.
4. Sao lưu, xác minh backup và chạy [script nâng cấp workflow](MedicalEquipmentManagement/Database/03_FinalCandidate_Workflow.sql) theo [hướng dẫn schema](Docs/DATABASE/SCHEMA_CHANGES.md). Các script seed Beta có dòng chọn database gốc. Khi dựng lại môi trường, đọc và khóa đích SQL trước khi chạy.
5. Build solution và khởi động IIS Express. Dùng tài khoản QA do quản trị viên cấp. Mật khẩu thật và thông tin phiên được giữ ngoài repository.

## Tài liệu

- [Hướng dẫn người dùng và Postman](Docs/USER_GUIDE.md)
- [Kiến trúc, mã nguồn và câu hỏi bảo vệ](Docs/SOURCE_CODE_GUIDE.md)
- [Đối chiếu Issues](Docs/PLANNING/REQUIREMENTS_TRACEABILITY.md)
- [Báo cáo kiểm thử](Docs/QA/FINAL_TEST_REPORT.md) và [ghi chú phát hành](Docs/RELEASE_NOTES.md)
- [Tiến độ bàn giao](Docs/FINAL_PROGRESS.md) và [changelog](CHANGELOG.md)
## Chạy bản Navbar cổ điển

Lần đầu, chạy `git fetch origin fix/rc-classic-ui` rồi `git switch -c fix/rc-classic-ui FETCH_HEAD`. Nếu đã có nhánh cục bộ, dùng `git switch fix/rc-classic-ui` và `git pull --ff-only origin fix/rc-classic-ui` khi worktree sạch. Các lệnh chỉ rõ nhánh để dùng được cả với bản clone chỉ theo dõi beta-projects. Mở solution, Restore NuGet, build và chạy IIS Express với BetaTest hiện tại. Nhánh kế thừa xử lý tài khoản và nghiệp vụ từ `dde840f`. Cập nhật từ bản đó chỉ cần mã nguồn, bỏ qua bước nâng cấp schema.

[Hướng dẫn nghiệm thu](Docs/QA/MANUAL_ACCEPTANCE_GUIDE.md) · [Dữ liệu mẫu](Docs/QA/TestData/README.md) · [Quy chuẩn đóng góp](CONTRIBUTING.md)
