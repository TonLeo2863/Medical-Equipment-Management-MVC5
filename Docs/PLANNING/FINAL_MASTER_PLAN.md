# Kế hoạch hoàn thiện ứng dụng

Nền `origin/beta-projects` tại `c9ce363d4aae4ec690054c366f798cfa63ee9d24`. Nhánh thực hiện `release/final-candidate` dựa trên GitHub HEAD đã fetch. Kiểm thử có ghi dữ liệu giới hạn tại `localhost\SQLEXPRESS`, `MedicalEquipmentDB_BetaTest`.

| Mốc | Công việc | Điều kiện nghiệm thu |
|---|---|---|
| 0 | Git, Issues/PR, schema và backup | SHA khớp GitHub, schema quan sát trực tiếp, backup qua VERIFYONLY. |
| 1 | Đăng nhập, vòng đời phiên, tài khoản và phân quyền | Hash cũ vẫn xác minh, benchmark có số đo, quyền được từ chối trực tiếp tại server, quản trị viên cuối cùng được bảo vệ. |
| 2 | Danh mục và tài sản | CRUD, lỗi dòng Excel, khớp Serial trên QR/barcode, vị trí thay đổi qua phiếu, lịch sử bảo toàn. |
| 3 | Phòng ban, kho và điều chuyển | Nguồn đúng vị trí, trạng thái hợp lệ, giao dịch nguyên tử, kiểm thử tranh chấp và rollback. |
| 4 | Công việc sửa chữa, bảo trì và hiệu chuẩn | STAFF báo hỏng qua phiên, phân công đúng kỹ sư, chuyển trạng thái tuần tự, lịch mới nhất quyết định cảnh báo. |
| 5 | Dashboard, thống kê và API | Số liệu khớp SQL, biểu đồ cục bộ, API kiểm tra quyền, token và trả DTO. |
| 6 | Giao diện | Menu theo vai trò, tiếng Việt, biểu mẫu và bảng dùng được trên các kích thước màn hình. |
| 7 | Kiểm thử Windows | Restore, Debug/Release, Razor, IIS Express, ma trận quyền và hồi quy có bằng chứng. |
| 8 | Tài liệu và Git | Traceability, báo cáo QA, hướng dẫn, kế hoạch nhánh và push mốc ổn định. |

Tiến độ thực tế cập nhật trong [FINAL_PROGRESS.md](../FINAL_PROGRESS.md). Kết quả Beta là dữ liệu tham chiếu cho hồi quy.

Mốc triển khai đã có trên nhánh phát hành. Debug, Release, Razor và hồi quy tích hợp 207 ca đã đạt. Các điều kiện cần thêm bằng chứng trước khi công nhận Final Candidate được ghi tại báo cáo QA. [Kết quả kiểm thử](../QA/FINAL_TEST_REPORT.md)
