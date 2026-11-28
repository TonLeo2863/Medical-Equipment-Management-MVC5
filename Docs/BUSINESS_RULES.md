# Quy tắc nghiệp vụ

- **ADMIN** quản trị tài khoản, phòng ban và các module. **MANAGER** quản lý danh mục, tài sản, kho, phân công và thống kê. **ENGINEER** xử lý phiếu được giao và ghi kỹ thuật. **STAFF** tra cứu, báo hỏng, xem phiếu của mình.
- Backend xác minh Session, Cookie gắn đăng nhập, tài khoản hoạt động và role hiện hành. Các thao tác ghi yêu cầu CSRF, kể cả Web API dùng Cookie.
- Mật khẩu mới từ 12 đến 256 ký tự và có xác nhận. Các trường mật khẩu được bind nguyên văn để băm, gồm ký tự giống HTML. Dùng PBKDF2-SHA256 600.000 vòng và hash tương thích hiện có.
- Tài sản đã phân bổ đổi khoa bằng phiếu điều chuyển. Khóa dòng tài sản, kiểm tra khoa nguồn và đích, rồi lưu trong một transaction. Thiết bị hỏng, bảo trì, thanh lý hoặc xóa mềm không được điều chuyển.
- Người báo phiếu lấy từ Session. Kỹ sư được phân công riêng. Kết thúc phiếu ghi kết quả và người thao tác. Thiết bị còn phiếu mở hoặc kết quả kỹ thuật không đạt chưa được đưa lại vào sử dụng.
- Tài sản có lịch sử bảo trì hoặc phiếu mở được bảo vệ khi xóa. Xóa mềm giữ lịch sử. Dashboard và cảnh báo dùng LINQ với lịch mới nhất theo ngày rồi theo ID.
