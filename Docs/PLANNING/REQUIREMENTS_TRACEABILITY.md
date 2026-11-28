# Đối chiếu yêu cầu và nghiệm thu

| Issue | Nền Beta và khoảng trống | Thay đổi trên nhánh phát hành | Tiêu chí nghiệm thu |
|---|---|---|---|
| [#1 · Tài khoản và giao diện](https://github.com/TonLeo2863/Medical-Equipment-Management-MVC5/issues/1) | Đăng nhập Session, đăng xuất GET, thiếu giao diện quản trị và kiểm tra lại trạng thái. BETA-004 mất 16–24 giây. | CNG, POST logout, đổi/đặt lại mật khẩu, quản trị người dùng, kiểm tra quyền ở server và menu. | Đăng nhập bốn vai trò, mật khẩu sai và tài khoản khóa bị từ chối, thao tác có CSRF, không vô hiệu hóa ADMIN cuối. |
| [#2 · Danh mục và thiết bị](https://github.com/TonLeo2863/Medical-Equipment-Management-MVC5/issues/2) | CRUD đã có. Quy tắc xóa MVC/API khác nhau, thiếu kiểm tra các action ghi và đầu vào Excel. | Chung quy tắc MVC/API, giữ lịch sử, giới hạn đầu vào Excel, hành động theo vai trò, QR/Barcode theo Serial. | CRUD đúng khóa ngoại, tìm/lọc/sắp xếp/phân trang khớp dữ liệu, kiểm từng dòng Excel và nội dung mã. |
| [#3 · Phòng ban và kỹ thuật](https://github.com/TonLeo2863/Medical-Equipment-Management-MVC5/issues/3) | Điều chuyển đã có. WorkOrder dùng cột người báo cho người được giao. Chưa có chức năng hiệu chuẩn. | Migration tác nhân, giới hạn chuyển trạng thái, lịch bảo trì và hiệu chuẩn, lịch sử tài sản, khóa dòng điều chuyển. | STAFF tạo phiếu, MANAGER phân công, ENGINEER xử lý, tài sản và lịch sử nhất quán, ghi người thực hiện và cảnh báo đúng. |
| [#4 · Dashboard và Web API](https://github.com/TonLeo2863/Medical-Equipment-Management-MVC5/issues/4) | Dashboard dùng số dự phòng. Web API thiết bị đã có quyền và antiforgery ở Beta. | Dashboard truy vấn EF6, Chart.js cục bộ, KPI theo quyền, báo cáo chi phí, chuẩn hóa thay đổi và Postman. | Số liệu khớp SQL và thay đổi theo dữ liệu, biểu đồ và bộ lọc hoạt động, mã API đúng tình huống. |

Bằng chứng chạy và phần còn thiếu được ghi theo từng ca trong báo cáo QA. Phần mở rộng chưa triển khai gồm Remember me, tự phục hồi mật khẩu, API ghi phiếu từ bên thứ ba và quan hệ trực thuộc Users–Departments.

| Issue | Bằng chứng thực thi |
|---|---|
| #1 | Bốn vai trò QA tạm, POST logout, Session hết hạn, đổi mật khẩu và bố cục 390/768/1280 px. Còn ca ADMIN cuối cùng và đăng nhập bằng tài khoản QA có sẵn. |
| #2 | CRUD danh mục/tài sản, chặn xóa có tham chiếu, đối chiếu phân trang, Excel và giải mã QR/Barcode đúng Serial. |
| #3 | Điều chuyển cạnh tranh, kiểm tra rollback SQL, STAFF báo hỏng, phân công và hoàn tất, bảo trì và hiệu chuẩn với đối chiếu trạng thái. |
| #4 | Trình duyệt render ba biểu đồ, đối chiếu số thiết bị và 12 tháng chi phí, kiểm thử mã Web API và token. Postman Desktop chưa chạy. |

[Báo cáo kiểm thử](../QA/FINAL_TEST_REPORT.md)
