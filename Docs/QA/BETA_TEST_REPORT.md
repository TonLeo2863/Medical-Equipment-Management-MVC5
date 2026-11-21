# Báo cáo kiểm thử Beta

**Beta candidate / not ready**. Mã chạy kiểm thử tại `6870b41c9d962966b2eab5a50b056811af57635f`. Windows 10 build 19044, Visual Studio 2022 Community MSBuild 17.14.60, .NET Framework 4.8, IIS Express, SQL Server Express 16.0.1000.6. Thực thi ngày 21/11/2026, múi giờ UTC+7.

| Nhóm hoặc ca | Trạng thái | Bằng chứng |
|---|---|---|
| NuGet restore packages.config | PASS | MSBuild /t:Restore /p:RestorePackagesConfig=true. Exit 0 trên bản sao đường dẫn ngắn. Logs/restore-final.log. |
| Build Debug, Release với Any CPU | PASS ×2 | MSBuild /p:Configuration=Debug hoặc Release /p:Platform="Any CPU". Hai lần exit 0. Logs/build-debug-retry.log và build-release-retry.log. |
| Razor precompile | PASS | Framework64 aspnet_compiler -v / -p đường dẫn dự án tạm. Exit 0. Logs/razor.log. |
| Kiểm tra đích và quyền SQL runtime | PASS | Kết nối thực tế trả MedicalEquipmentDB_BetaTest và sysadmin=0. Logs/http-final.log. |
| Schema và seed lần một | PASS ×3 | Database.sql, CapNhatDatabase_Module3.sql, 02_Seed_Module2.sql. Ba lần exit 0. Logs/sql-1-*.log. |
| Schema và seed lần hai | PASS ×2, FAIL ×1 | Module 3 và seed Module 2 exit 0. Database.sql exit 1 với Msg 2627. Logs/sql-2-*.log. |
| Smoke HTTP | PASS ×9 | Home, About, trang lỗi 404/500, guest Equipment redirect 302, guest API 401, hai lần GET Login, POST Login sai trả 200. Lần đăng nhập sai chỉ kiểm tra mã HTTP. Logs/http-final.log. |
| Login thành công | BLOCKED | IIS ghi nhận POST Admin nhưng phiên thực thi kết thúc trước khi ghi phản hồi. Chưa có kết luận cho Admin hay các vai trò còn lại. Logs/iis-stdout.log. |
| Session, logout, ma trận GET/POST theo vai trò | NOT RUN | Bộ smoke chưa đi tới các ca cần đăng nhập. |
| CRUD MVC, dữ liệu lỗi, FK, soft delete, tìm/lọc/sắp xếp/phân trang | NOT RUN | Chưa có phiên đăng nhập xác nhận. |
| Điều chuyển, sai nguồn/đích, bảo trì, lịch sử và rollback | NOT RUN | Chưa có phiên đăng nhập xác nhận. |
| Maintenance và WorkOrder | NOT RUN | Chưa có phiên đăng nhập xác nhận. |
| Import Excel, QR/Barcode | NOT RUN | Chưa thử upload, mẫu lỗi, giới hạn kích thước hoặc dữ liệu ảnh. |
| API xác thực GET/POST/PUT/DELETE, JSON, validation và kiểm tra dữ liệu | NOT RUN | Chưa có phiên đăng nhập xác nhận. |
| Responsive, dấu tiếng Việt, toàn bộ menu và liên kết | NOT RUN | Smoke chỉ xác nhận mã trạng thái. |
| Đối chiếu đầy đủ mapping EF với SQL | NOT RUN | Đã tạo schema nhưng chưa xác minh đủ truy vấn ứng dụng theo bảng. |

**Tổng lượt kiểm tra cuối** 19 PASS, 1 FAIL, 1 BLOCKED và 8 nhóm NOT RUN. Restore và build ban đầu đã lỗi vì mạng sandbox và đường dẫn quá dài. Các kết quả trước khi phục hồi không được đếm lại.

## SQL preflight và chạy lại

Đã rà tất cả SQL/config được Git theo dõi cho USE, CREATE/DROP/ALTER DATABASE, DELETE, TRUNCATE, UPDATE và connection string. Ba file SQL có USE trỏ database gốc. Database.sql còn có CREATE DATABASE và các UPDATE seed ở dòng 247, 316, 331 và 339. Web.config gốc có tên máy cụ thể và đích DB gốc. Hai transform có ví dụ connection string trong comment. Views/Web.config và packages.config không có kết nối runtime.

Tạo mới riêng `MedicalEquipmentDB_BetaTest` sau khi xác minh chưa tồn tại. Tạo ba bản sao SQL UTF-8 trong thư mục tạm, bỏ CREATE DATABASE gốc và mọi USE, thêm `IF DB_NAME() <> N'MedicalEquipmentDB_BetaTest' THROW 51000, 'Unexpected database', 1` và `EXECUTE AS USER = 'beta_schema_qa'` trước batch đầu. User thử nghiệm có CREATE TABLE và quyền ALTER, REFERENCES, SELECT, INSERT, UPDATE trên schema dbo trong DB thử nghiệm. Chạy lần lượt các bản sao bằng `sqlcmd -S lpc:.\SQLEXPRESS -d MedicalEquipmentDB_BetaTest -b -f 65001 -i <file>` và lặp lại cùng thứ tự. Log mỗi script xác nhận database và effective user. Dùng login ứng dụng tạm có quyền DML trên dbo, sửa Web.config ở bản sao IIS và tạo bốn tài khoản QA tổng hợp. Lượt hai của Database.sql có thể thay đổi dữ liệu demo trước hoặc sau các lỗi khóa trùng. Dữ liệu đó thuộc DB thử nghiệm.
