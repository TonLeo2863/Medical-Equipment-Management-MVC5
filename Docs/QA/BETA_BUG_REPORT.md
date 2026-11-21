# Báo cáo lỗi Beta

## BETA-001 — Seed cơ sở không an toàn khi chạy lặp

- **P1 · Database / seed · OPEN**. Nhánh beta-projects, commit 6870b41c9d962966b2eab5a50b056811af57635f. Đề xuất Module 3 và người phụ trách database.
- **Vị trí** `MedicalEquipmentManagement/Database/Database.sql:241` và các INSERT tiếp theo. Dòng 331 trừ tồn kho mỗi lần chạy và cần kiểm tra khi sửa.
- **Môi trường và tái hiện** SQL Server Express 16.0.1000.6. Trên một DB Beta mới, chạy seed theo thứ tự trong BETA_TEST_REPORT.md rồi chạy lại bản sao Database.sql với `sqlcmd -b`. Đầu vào là dữ liệu demo mặc định.
- **Mong đợi** Chạy lần hai trả 0, không tạo bản ghi trùng và không đổi số lượng tồn kho.
- **Thực tế** Lần hai trả 1. SQL trả Msg 2627, gồm khóa trùng của EquipmentAssets với mã SA-001. Log còn ghi có câu lệnh thay đổi dòng.
- **Bằng chứng** [SQL lượt 2](Logs/sql-2-Database.sql.log).
- **Nguyên nhân và hướng sửa** INSERT seed không có điều kiện theo khóa nghiệp vụ, một số UPDATE có hiệu ứng tích lũy. Tách seed và áp dụng điều kiện theo khóa cùng transaction. Chạy hai lượt trên DB thử mới và đối chiếu số bản ghi cùng số lượng tồn.

## BETA-002 — Phiên QA chưa thu được phản hồi đăng nhập hợp lệ

- **P1 · Account / session MVC · BLOCKED**. Nhánh beta-projects, commit 6870b41c9d962966b2eab5a50b056811af57635f. Đề xuất Module 1 và QA.
- **Vị trí** `MedicalEquipmentManagement/Controllers/AccountController.cs`, action POST Login. Chưa xác định dòng lỗi.
- **Môi trường và tái hiện** IIS Express localhost với SQL Express và tài khoản demo Admin có PBKDF2. GET Login để lấy cookie cùng antiforgery token, gửi POST Login với thông tin đúng.
- **Mong đợi** HTTP 302 về Home, sau đó truy cập được trang yêu cầu session.
- **Thực tế** Log có sự kiện bắt đầu POST. Phiên chạy bị gián đoạn trước khi lưu status. Tình trạng runtime của request chưa xác định, đây là trở ngại QA.
- **Bằng chứng** [IIS runtime](Logs/iis-stdout.log) và [HTTP smoke](Logs/http-final.log).
- **Hướng xử lý** Chạy lại cùng fixture bằng tiến trình IIS ổn định, ghi thời gian xử lý truy vấn và PBKDF2 nếu request vẫn không kết thúc. Tiếp tục các ca phân quyền và nghiệp vụ sau khi thu được phản hồi.
