# Báo cáo lỗi nhánh phát hành

| Mã | Mức độ | Hiện tượng và nguyên nhân | Thay đổi và bằng chứng |
|---|---|---|---|
| BETA-004 | P2 | Đăng nhập Beta mất khoảng 16–24 giây. Dẫn xuất PBKDF2 managed chiếm phần lớn thời gian. | Dùng Windows CNG với cùng tham số và định dạng. Đối chiếu vector và số đo trong [benchmark](LOGIN_BENCHMARK.md). Theo dõi tiếp thời gian cold. |
| BETA-005 | P1 | POST WorkOrder/Create bằng STAFF trên Beta `c9ce363` trả HTTP 500 khi để kỹ sư trống. Chuỗi lỗi tại SaveChanges gồm `DbUpdateException → UpdateException → SqlException` do SQL 515 từ chối NULL ở `ReportedBy`. | Tách người báo khỏi kỹ sư, lấy người báo từ Session và giữ người phân công nullable. RC tạo phiếu thành công và SQL xác nhận các giá trị. [Log tái hiện Beta](Logs/final-beta005-reproduction.log) · [Schema](../DATABASE/SCHEMA_CHANGES.md) |
| RC-001 | P1 | Ràng buộc xóa MVC và API khác nhau. Có action ghi tài sản chưa kiểm tra quyền cụ thể. | Thống nhất xóa mềm và bảo vệ lịch sử bảo trì, khóa thay đổi vị trí và xác nhận role ở server. Kết quả theo [báo cáo QA](FINAL_TEST_REPORT.md). |
| RC-002 | P2 | GET Logout rơi vào error redirect thay vì từ chối phương thức rõ ràng. | GET trả 405 và giữ phiên, POST có token hủy phiên. Retest trên IIS đã đạt. |
| RC-003 | P2 | Lượt QA mở rộng dừng ở phép so nội dung tiếng Việt trên HTML chưa giải mã. | Giải mã HTML trước khi so. Retest đã xác nhận lỗi đúng dòng và SQL chỉ ghi dòng hợp lệ. [Log IIS](Logs/final-rc-iis.log) |
| RC-004 | P3 | Harness PowerShell lồng mảng JSON, và truy vấn đối chiếu EquipmentModel dùng nhầm tên thuộc tính EF làm tên cột SQL. | Đọc mảng chi phí trực tiếp, truy vấn cột SQL `Name`. [Báo cáo QA](FINAL_TEST_REPORT.md) |

Kết quả QA Beta của seed và cấu hình handler Web API thuộc BETA-001 đến BETA-003. [Báo cáo Beta](BETA_BUG_REPORT.md)

## RC-005–011 · Sửa nghiệm thu

| Mã | Mức độ | Nguyên nhân kiểm chứng | File và cách sửa |
|---|---|---|---|
| RC-005 | P1 | MVC request validation chặn mật khẩu dài hợp lệ chứa chuỗi giống HTML trước khi đến action tạo. Form thiếu xác nhận và lỗi từng trường khiến khó phân biệt với lỗi dữ liệu. | `AccountInputs`, `UserAdminController`, `Views/UserAdmin/Edit` chỉ cho binding nguyên văn trường mật khẩu, yêu cầu xác nhận và báo tiếng Việt tại trường. |
| RC-006 | P1 | Trên bản nền, gửi mật khẩu có dấu ngoặc góc bị chặn; hash mới chưa được lưu. Luồng reset và đăng nhập phải dùng cùng quy tắc binding. | `NewPasswordInput`, `LoginInput`, view Reset và Login. Tiếp tục dùng PBKDF2 và cơ chế thu hồi phiên qua dấu xác thực. |
| RC-007 | P1 | MVC chặn trường mật khẩu hiện tại, mới hoặc xác nhận nếu có chuỗi giống HTML. Các lỗi xác minh cần hiển thị cạnh ô nhập. | `ChangePasswordInput`, `AccountController` và view ChangePassword. Giữ cập nhật có điều kiện để tránh ghi đè lần đặt lại đồng thời. |
| RC-008 | P2 | Nút, trường nhập và select dùng kiểu cũ khác nhau, mũi tên dựa giao diện gốc trình duyệt. | `Site.css` dùng Segoe UI/system-ui và SVG thống nhất trong select một lựa chọn. |
| RC-009 | P2 | Menu nhóm đặt tuyệt đối trên Navbar ngang và dùng chiều rộng theo nội dung. Nhãn tài khoản hoặc mục dài có thể vượt viewport. | `_Navbar`, `_Layout`, `Site.css`, `site.js` dùng nhóm mở trong vùng cuộn của Sidebar, tên dài được xuống dòng. |
| RC-010 | P2 | Nhóm nút Import và lưới tra cứu dùng chiều rộng cứng; Edge WebView2 phát hiện tràn ở 360 px. | `Import.cshtml` và CSS co giãn, menu di động, bảng cuộn riêng và input từ 16 px. |
| RC-011 | P2 | TempData hiển thị theo danh sách controller và các view tự vẽ banner riêng, dễ mất thông báo khi redirect sang trang khác. | Layout đọc mỗi khóa một lần, banner có nhãn chữ/ARIA. Validation vẫn bám từng trường, thêm trạng thái đang xử lý và xác nhận thao tác ảnh hưởng dữ liệu. |

Ba ca mật khẩu được tái hiện trên commit nền `793e386` bằng dữ liệu tổng hợp; mẫu chữ/số đối chứng vẫn qua. Các dữ liệu đầu vào người dùng từng nhập chưa được cung cấp. Xem chứng cứ retest riêng trong [báo cáo kỹ thuật](FINAL_TEST_REPORT.md) và trạng thái người dùng trong [biên bản nghiệm thu](USER_ACCEPTANCE_REPORT.md).
