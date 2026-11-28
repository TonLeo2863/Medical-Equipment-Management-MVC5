# Ghi chú nhánh phát hành

Nhánh `release/final-candidate` tích hợp trên Beta để kiểm thử trước bàn giao. Trạng thái từng ca QA và các điều kiện còn mở được ghi trong [báo cáo kiểm thử](QA/FINAL_TEST_REPORT.md).

- Tài khoản có quyền theo role, đổi và đặt lại mật khẩu, tái xác nhận phiên khi quyền/trạng thái thay đổi, đăng xuất POST chống CSRF và PBKDF2 tương thích hash Beta.
- Điều chuyển cập nhật vị trí và lịch sử trong transaction. Phiếu sửa chữa lưu riêng người báo, kỹ sư và người kết thúc. Bảo trì và hiệu chuẩn có lịch sử, kết quả và ngày kế tiếp.
- Tài sản dùng quy tắc chung cho MVC và API. Nhập Excel có thông báo theo dòng. Dashboard thực hiện truy vấn EF6 và dùng Chart.js cục bộ.
- Thực hiện backup, xác minh và chạy migration workflow trên BetaTest trước khi khởi động bản mới. [Thay đổi schema](DATABASE/SCHEMA_CHANGES.md)

Phần mở rộng chưa triển khai gồm tự phục hồi mật khẩu, Remember me, dùng phòng ban để phân vùng người dùng và tạo phiếu sửa chữa từ ứng dụng ngoài. Việc dọn nhánh nguồn phụ thuộc trạng thái PR và kết quả nghiệm thu. [Kế hoạch nhánh](BRANCH_CLEANUP_PLAN.md)