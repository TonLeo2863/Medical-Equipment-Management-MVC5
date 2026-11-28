# Báo cáo kiểm thử nhánh phát hành

Ứng dụng `24a0244cc9f67d8419da4ec3015ee0fd181cf7d7`. Môi trường Windows, Visual Studio 2022 MSBuild, .NET Framework 4.8, IIS Express, SQL Express và database `MedicalEquipmentDB_BetaTest`. Các bài ghi dữ liệu dùng fixture tạm. Tài khoản QA có từ trước và dữ liệu ngoài fixture được giữ nguyên.

Lượt hồi quy tích hợp cuối exit 0 với **207 kiểm tra PASS và 0 FAIL**. Đếm theo dòng khẳng định, trong đó phép kiểm quyền GET/POST có thể kiểm hai request trong một dòng. Hai ca tái hiện Beta và hai ca Session timeout chạy riêng, không cộng vào tổng tích hợp. [Log hồi quy tích hợp](Logs/final-integration.log)

## Kết quả có bằng chứng

| Nhóm | Trạng thái | Bằng chứng |
|---|---|---|
| NuGet Restore theo `packages.config` | PASS | Restore mạng exit 0, giữ nguyên phiên bản package. |
| MSBuild Debug, MSBuild Release, Razor precompile | PASS | Cả hai cấu hình build và `aspnet_compiler -v / -p MedicalEquipmentManagement` chạy thành công trên nguồn hiện tại. |
| Backup, schema và SQL | PASS | COPY_ONLY có checksum, VERIFYONLY thành công, migration chạy hai lượt và bốn khóa ngoại được tin cậy. [Chi tiết schema](../DATABASE/SCHEMA_CHANGES.md) |
| Khởi động IIS Express, bốn vai trò và phân quyền | PASS trong phạm vi đã thực thi | Đăng nhập bốn vai trò QA tạm, mật khẩu sai, tài khoản vô hiệu hóa, POST logout, GET logout 405, Cookie HttpOnly, chỉ Session ID không đủ. Ma trận từ chối GET/POST kiểm tra danh mục, tài sản, phòng ban, quản trị, điều chuyển, bảo trì, hiệu chuẩn và phiếu. |
| Hết hạn Session sau thời gian không hoạt động | PASS | Bản sao IIS có timeout 1 phút, dùng cùng Cookie jar. API trả 200 trước hạn và 401 sau 75 giây. [Log Session](Logs/final-session-expiry.log) |
| Quản trị người dùng | PASS trong phạm vi đã thực thi | Tạo tài khoản, đặt lại mật khẩu, đăng nhập bằng mật khẩu mới, đổi mật khẩu cá nhân, bỏ qua trường RoleId tự gửi, vô hiệu hóa, kích hoạt lại. |
| Tìm kiếm, phân trang, tài sản và API | PASS trong phạm vi đã thực thi | Trang 1 và 2 theo giá khớp SQL. API GET và các mã 200/201/204/400/401/403/404/409. Từ chối PUT đổi khoa, giá âm, xóa tài sản có lịch sử bảo trì, thao tác ghi thiếu token. |
| Điều chuyển và rollback | PASS | Hai yêu cầu HTTP đồng thời chỉ ghi một phiếu và một giao dịch kho. Fixture SQL phát sinh lỗi FK 547 và rollback, giữ đúng vị trí trước transaction lỗi. |
| BETA-005 và vòng đời phiếu | PASS | Beta cũ trả HTTP 500 và ghi đúng chuỗi exception. RC STAFF tạo phiếu không có kỹ sư, MANAGER phân công và ENGINEER hoàn tất. Còn phiếu mở thì thiết bị chưa hoạt động lại. ADMIN hủy phiếu chưa phân công và giữ lịch sử. |
| Bảo trì và hiệu chuẩn | PASS trong phạm vi đã thực thi | Ghi kỹ thuật viên/liên kết phiếu và SQL đối chiếu. Hiệu chuẩn không đạt đánh dấu hỏng và chặn đưa lại vào hoạt động. Kiểm tra danh sách 15 ngày. |
| Dashboard và thống kê | PASS trong phạm vi đã thực thi | So số thiết bị, từng trạng thái và chi phí cả 12 tháng với SQL. Cảnh báo có fixture. Tài nguyên Chart.js phục vụ cục bộ. |
| Excel và mã nhận diện | PASS | Mẫu Excel tải được. File hợp lệ chỉ lưu dòng đúng, lỗi báo theo dòng đối với giá âm và mã trùng. Từ chối tệp hỏng và vượt 2 MB. ZXing đọc QR và Barcode đúng Serial. |
| CRUD Category, Supplier, EquipmentModel, Department và EquipmentAsset qua MVC | PASS | Lượt danh mục đạt 33 kiểm tra. Xác minh khóa ngoại, dữ liệu SQL, xóa mềm và từ chối đổi khoa trực tiếp. [Log danh mục](Logs/final-catalog.log) |
| Responsive và tiếng Việt | PASS trong phạm vi đã thực thi | Giải mã UTF-8 nghiêm ngặt trên 113 tệp first-party. Edge WebView2 kiểm tra màn hình đăng nhập và Dashboard/Thiết bị tại 390, 768 và 1280 px, menu di động và ba biểu đồ render với dữ liệu SQL. [Log responsive](Logs/final-responsive.log) |

[Bằng chứng IIS](Logs/final-integration.log) · [Lượt trước khi sửa harness](Logs/final-rc-iis.log) · [Tái hiện BETA-005](Logs/final-beta005-reproduction.log) · [Benchmark](LOGIN_BENCHMARK.md)

## Lỗi phát sinh khi kiểm thử

- FAIL ở lượt đầu khi so thông báo Excel với HTML chưa giải mã. Sau khi cập nhật phép so, retest xác nhận thông báo và dữ liệu lưu khớp.
- FAIL ở lượt mở rộng do PowerShell lồng mảng chi phí JSON. Đọc mảng trực tiếp đã PASS toàn bộ 12 tháng.
- FAIL trong harness khi SQL dùng `ModelName`. EF ánh xạ thuộc tính này sang cột `Name`. Retest phép đối chiếu và toàn bộ CRUD danh mục đã đạt.

## Phạm vi chưa có bằng chứng đầy đủ

- BLOCKED xác thực bằng chính tài khoản QA có từ trước vì phiên làm việc chưa có thông tin đăng nhập. Bốn vai trò đã được xác minh bằng tài khoản QA tạm.
- NOT RUN seed/schema trên DB trống riêng và restore vật lý. Phạm vi SQL được phép ghi là BetaTest có dữ liệu.
- NOT RUN ca vô hiệu hóa hoặc hạ quyền ADMIN hoạt động duy nhất. BetaTest có nhiều ADMIN hiện hữu và việc kiểm thử giữ nguyên các tài khoản đó. Kiểm thử đã xác minh vô hiệu hóa tài khoản fixture làm mất phiên ngay ở API.
- NOT RUN ma trận đọc chi tiết và lịch sử cho mọi kiểu dữ liệu và vai trò. Ma trận từ chối thao tác ghi và ca ghi hợp lệ của các module chính đã thực thi.
- NOT RUN chạy Postman Desktop. Các phương thức đã thực thi cùng request và token bằng HttpClient; collection có hướng dẫn trong repository.

## Chạy lại

Build Release rồi dùng PowerShell trên máy Windows có SQL Express/IIS Express. Chạy `powershell.exe -NoProfile -ExecutionPolicy RemoteSigned -File Docs/QA/Run-FinalSmoke.ps1`. Có thể đặt `MEDICAL_QA_BARCODE_DECODER` đến DLL ZXing.Net net48 để giải mã ảnh. Để kiểm tra trình duyệt, đặt `MEDICAL_QA_WEBVIEW_EXECUTABLE` đến bản WinExe x64 của `Verify-Responsive.cs`, build với bộ thư viện net462 và `WebView2Loader.dll` x64 của SDK WebView2 1.0.3351.48. Dùng `-CatalogOnly` hoặc `-BrowserOnly` khi cần chạy lại đúng nhóm. Script kiểm tra cấu hình SQL, `DB_NAME()` và dọn fixture theo khóa của lần chạy. [SDK WebView2](https://www.nuget.org/packages/Microsoft.Web.WebView2/1.0.3351.48)
