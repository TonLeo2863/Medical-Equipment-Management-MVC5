# Biên bản nghiệm thu người dùng

Nguồn là kết quả người dùng cung cấp cho đợt `fix/rc-acceptance-ui`. Mức đánh giá hiện tại **5/10**. Mốc mục tiêu ít nhất 8/10 do người dùng xác nhận sau khi thử giao diện và chức năng.

| Lượt | PASS | FAIL | NOT RUN |
|---|---:|---:|---:|
| Ban đầu | 34 | 8 | 49 |
| Sau khi phân loại lại DEV-14 do thiếu công cụ quét | 34 | 7 | 50 |

Đã được người dùng xác nhận gồm khởi tạo database, Restore NuGet, build/chạy, đăng nhập bốn vai trò, đăng nhập sơ bộ khoảng 3 giây, CRUD danh mục/nhà cung cấp/mẫu/khoa, tìm/lọc/sắp xếp/phân trang, vòng đời STAFF → MANAGER → ENGINEER và tiếng Việt hiển thị. Nhóm tiếp tục 57 ca FAIL/NOT RUN theo checklist riêng.

| Mã | Phản hồi đã nhận | Trạng thái thủ công trước retest |
|---|---|---|
| RC-005 | Tạo tài khoản không lưu | FAIL |
| RC-006 | Mật khẩu đặt lại không đăng nhập được | FAIL |
| RC-007 | Mật khẩu tự đổi không đăng nhập được | FAIL |
| RC-008 | Font và mũi tên dropdown lệch nhau | FAIL |
| RC-009 | Menu các nhóm bị tràn | FAIL |
| RC-010 | Điện thoại chưa thích ứng tốt | FAIL |
| RC-011 | Thông báo quá nhỏ hoặc mất | FAIL |
| DEV-14 | Chưa có công cụ quét | NOT RUN |

Kết quả tự động do nhóm phát triển ghi ở [báo cáo kỹ thuật](FINAL_TEST_REPORT.md). Khi nhận checklist thành viên, đối chiếu mã ca hoặc triệu chứng trước khi tạo lỗi. Ghi SHA, môi trường, vai trò, dữ liệu đã ẩn danh, mức độ, nguyên nhân và bằng chứng retest vào dòng hiện có nếu trùng. Ca thiếu công cụ hoặc điều kiện giữ BLOCKED/NOT RUN phù hợp. Mỗi vòng fix chỉ yêu cầu người dùng thử lại ca bị ảnh hưởng và luồng liền kề.
