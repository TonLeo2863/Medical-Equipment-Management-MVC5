# Nhật ký thay đổi

## Navbar cổ điển · fix/rc-classic-ui

- Khôi phục Navbar ngang, nền sáng, màu xanh dương, khung nội dung 1120 px và minh họa đăng nhập theo giao diện `793e386`. Nhóm Danh mục và Kho có nút mở riêng. Thanh điều hướng thu gọn thành nút Menu trên màn hình hẹp.
- Dropdown neo theo mép phải, có thể mở bằng bàn phím và đóng bằng Escape. Select và nhãn dropdown dùng chung mũi tên SVG cục bộ. Biểu mẫu, thông báo và vùng cuộn bảng kế thừa bản sửa nghiệm thu `dde840f`.
- Mở rộng kiểm thử Edge WebView2 đến 1366 px và kiểm tra vị trí từng bảng chọn. Workflow Windows chạy cả trên nhánh Navbar. Bản cập nhật giao diện dùng schema hiện có.

## Sửa nghiệm thu RC

- Sửa RC-005–007 khi MVC nhận mật khẩu chứa ký tự giống HTML. Bổ sung xác nhận mật khẩu, lỗi tiếng Việt theo trường và nút hiện/ẩn.
- Sửa RC-008–011 với Sidebar, ngăn di động, SVG cho select và thông báo tập trung. Sửa tràn ngang ở trang Import, Điều chuyển và Chi tiết thiết bị trên viewport 360 px.
- Bổ sung fixture Excel, SQL chỉ đọc, hướng dẫn nghiệm thu, quy chuẩn phát triển và workflow build Windows. Schema giữ nguyên.

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
