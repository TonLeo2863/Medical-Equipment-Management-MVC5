# Ghi chú nhánh phát hành

## Bản Navbar cổ điển

Nhánh `fix/rc-classic-ui` kế thừa `fix/rc-acceptance-ui` tại `dde840f`. Giao diện trở về Navbar ngang, thẻ sáng, nền xám nhạt và màu nhấn xanh dương theo mốc `793e386`. Trên điện thoại hoặc tablet hẹp, bấm **Menu** để mở các nhóm ngay bên dưới thanh đầu trang.

Khi cập nhật từ bản Sidebar, chỉ cần chuyển nhánh, restore, build và chạy theo README. Form mật khẩu, cơ chế phiên, phân quyền, quy trình sửa chữa, dữ liệu Dashboard và Web API tiếp tục dùng mã nguồn của bản đã nghiệm thu kỹ thuật. Chart.js, font hệ thống và SVG chạy cục bộ. BetaTest sử dụng schema hiện có. [Ảnh đối chiếu](QA/Screenshots/README.md) · [Kết quả hồi quy](QA/FINAL_TEST_REPORT.md)

## Lịch sử bản Final Candidate

Nhánh `release/final-candidate` tích hợp trên Beta để kiểm thử trước bàn giao. Trạng thái từng ca QA và các điều kiện còn mở được ghi trong [báo cáo kiểm thử](QA/FINAL_TEST_REPORT.md).

- Tài khoản có quyền theo role, đổi và đặt lại mật khẩu, tái xác nhận phiên khi quyền/trạng thái thay đổi, đăng xuất POST chống CSRF và PBKDF2 tương thích hash Beta.
- Điều chuyển cập nhật vị trí và lịch sử trong transaction. Phiếu sửa chữa lưu riêng người báo, kỹ sư và người kết thúc. Bảo trì và hiệu chuẩn có lịch sử, kết quả và ngày kế tiếp.
- Tài sản dùng quy tắc chung cho MVC và API. Nhập Excel có thông báo theo dòng. Dashboard thực hiện truy vấn EF6 và dùng Chart.js cục bộ.
- Thực hiện backup, xác minh và chạy migration workflow trên BetaTest trước khi khởi động bản mới. [Thay đổi schema](DATABASE/SCHEMA_CHANGES.md)

Phần mở rộng chưa triển khai gồm tự phục hồi mật khẩu, Remember me, dùng phòng ban để phân vùng người dùng và tạo phiếu sửa chữa từ ứng dụng ngoài. Việc dọn nhánh nguồn phụ thuộc trạng thái PR và kết quả nghiệm thu. [Kế hoạch nhánh](BRANCH_CLEANUP_PLAN.md)
## Sửa nghiệm thu RC-005–011

Nhánh `fix/rc-acceptance-ui` sửa phạm vi binding mật khẩu có ký tự giống HTML, thêm xác nhận khi tạo, lỗi từng trường và nút hiện/ẩn. Bố cục dùng Sidebar có thể thu gọn, ngăn di động, font Segoe UI, mũi tên SVG, bảng cuộn và banner thông báo chung. Giữ cấu trúc database, định dạng hash, Session/Cookie, route và CSRF. Không cần chạy lại migration đã áp dụng.

Bộ mẫu Excel, truy vấn SQL chỉ đọc và hướng dẫn nghiệm thu được cung cấp cho thành viên. Biên bản nghiệm thu người dùng vẫn giữ kết quả đã nhận và chờ retest. [Hướng dẫn](QA/MANUAL_ACCEPTANCE_GUIDE.md) · [Biên bản](QA/USER_ACCEPTANCE_REPORT.md)
