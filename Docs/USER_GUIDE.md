# Hướng dẫn sử dụng

## Đăng nhập và thao tác theo vai trò

- Mở trang chủ, chọn **Đăng nhập** và dùng tài khoản do ADMIN cấp. Menu tài khoản có **Đổi mật khẩu** và **Đăng xuất**.
- **ADMIN** quản lý tài khoản, khoa/phòng và các module. Dùng **Quản lý tài khoản** để cấp vai trò, đặt lại mật khẩu hoặc vô hiệu hóa.
- **MANAGER** quản lý danh mục, kho, tài sản, phân công kỹ sư và xem báo cáo chi phí.
- **ENGINEER** mở **Phiếu sửa chữa** để tiếp nhận công việc đã được giao. **Kỹ thuật** tập hợp bảo trì và hiệu chuẩn.
- **STAFF** tra cứu thiết bị, chọn **Báo hỏng** và theo dõi phiếu của mình.

## Tài sản, điều chuyển và kỹ thuật

1. Tạo danh mục, nhà cung cấp và mẫu trước khi khai báo tài sản. Giữ mã tài sản và Serial duy nhất. Dùng mã ASCII cho Serial mới để in Barcode.
2. Khi có nhiều dòng, mở **Thiết bị → Import Excel**, tải mẫu và sửa các dòng ví dụ. Mã khoa có thể trống cho kho dự trữ. File `.xlsx` tối đa 2 MB và 2.000 dòng dữ liệu. Kiểm tra kết quả theo dòng trước khi tải lại.
3. Khoa đã gán được thay đổi trong **Kho và điều chuyển**. Chọn tài sản đang hoạt động, khoa nguồn thực tế và khoa đích đang hoạt động. Lịch sử ghi người thực hiện theo tài khoản đăng nhập.
4. STAFF chọn thiết bị và mô tả sự cố. MANAGER mở phiếu chờ để phân công kỹ sư. Kỹ sư bắt đầu sửa, ghi kết quả và chọn đạt hoặc không đạt khi hoàn tất.
5. Ghi bảo trì bằng ngày thực hiện, ngày kế tiếp, kết quả, chi phí tùy chọn và phiếu liên kết cùng thiết bị. Với hiệu chuẩn, nhập ngày, người/đơn vị, chứng nhận và kết quả. Xem lịch sử trên trang chi tiết tài sản.
6. Dashboard lọc theo khoa/phòng và hạn 15/30 ngày. ADMIN/MANAGER chọn năm thống kê chi phí. Danh sách ưu tiên hiển thị tối đa 30 cảnh báo gần nhất theo hạn, gồm hạn đã quá.

## Sử dụng Web API bằng Postman

Nhập [Postman Collection](QA/Equipment.postman_collection.json). Điền `baseUrl`, `username`, `password` bằng biến cục bộ, giữ Cookie jar cho cùng hostname. Chạy GET form Login rồi POST Login. Để thử ghi, chạy GET form tạo thiết bị và gửi `RequestVerificationToken` từ biến thu được. Chọn `modelId` hợp lệ, mã thử duy nhất và cập nhật `assetId` từ phản hồi tạo. Giữ mật khẩu, Cookie và token ngoài tài liệu lưu chung.

| HTTP | Ý nghĩa |
|---|---|
| 200, 201, 204 | Đọc, tạo và cập nhật/xóa mềm thành công. |
| 400 | Validation hoặc kiểm tra token thất bại. |
| 401, 403 | Phiên không hợp lệ hoặc vai trò không có quyền. |
| 404, 409 | Thiết bị thiếu/đã xóa hoặc phát sinh xung đột nghiệp vụ. |
## Giao diện và tài khoản trong bản sửa nghiệm thu

Trên máy tính, dùng Navbar ngang phía trên. Chọn **Danh mục**, **Kho**, **Kỹ thuật** hoặc **Tài khoản** để mở các chức năng theo vai trò. Trên màn hình hẹp, nút **Menu** mở các liên kết ngay dưới Navbar. Dùng Tab để di chuyển, Enter để mở nhóm và Escape để quay về nhãn nhóm. Nhấn Escape lần nữa để đóng Menu và trở về nút mở. Bảng rộng cuộn trong vùng bảng.

Khi cấp tài khoản, nhập mật khẩu từ 12–256 ký tự và xác nhận đúng. Nút **Hiện** cho phép kiểm tra ký tự trước khi gửi. Thông báo dưới từng ô giải thích dữ liệu cần sửa. Đặt lại mật khẩu có hộp xác nhận, và sau khi lưu các phiên dùng dấu xác thực cũ sẽ hết hiệu lực.

[Hướng dẫn nghiệm thu từng bước](QA/MANUAL_ACCEPTANCE_GUIDE.md)
