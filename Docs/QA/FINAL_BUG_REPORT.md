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
