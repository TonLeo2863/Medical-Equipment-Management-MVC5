# Nhật ký thay đổi

## Nhánh phát hành đang kiểm thử

- Bổ sung quản lý tài khoản, đổi mật khẩu và đăng xuất POST. Tái xác nhận quyền và trạng thái tài khoản từ SQL Server.
- Chuyển dẫn xuất hash PBKDF2-SHA256 sang Windows CNG, giữ nguyên định dạng và tham số Beta.
- Tách người báo, kỹ sư được phân công và người hoàn thành phiếu. Lưu kết quả kỹ thuật và ánh xạ phiếu bảo trì.
- Ghi hiệu chuẩn, chỉ xét lịch gần nhất khi cảnh báo, bổ sung Dashboard và biểu đồ chạy ngoại tuyến.
- Đồng bộ quy tắc xóa mềm và đổi vị trí tài sản giữa MVC và API. Tăng kiểm tra role, đầu vào Excel và dữ liệu mã vạch.
- Thêm script nâng cấp BetaTest, cột tác nhân và khóa ngoại cho workflow, giữ dữ liệu lịch sử.
- Cập nhật chú thích kỹ thuật, hướng dẫn vận hành và tài liệu bảo vệ đồ án bằng tiếng Việt.

## Beta tích hợp

- Tích hợp ba module tài khoản, danh mục, phòng ban và kỹ thuật.
- Bổ sung Web API thiết bị với Session, antiforgery và handler cho PUT/DELETE.
- Sửa dữ liệu seed để chạy lặp và bổ sung báo cáo thử nghiệm Beta.