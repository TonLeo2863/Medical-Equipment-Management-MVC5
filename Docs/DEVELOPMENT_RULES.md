# Quy chuẩn phát triển và review

| Module | Phạm vi | Người phối hợp review |
|---|---|---|
| Tài khoản và giao diện | Account, UserAdmin, Filter, Session, layout và CSS/JS chung | Người phụ trách module #1 và chủ repository |
| Danh mục và tài sản | Category, Supplier, EquipmentModel, Equipment, Import, QR/Barcode | Người phụ trách module #2 và người phụ trách Web API khi sửa quy tắc tài sản |
| Phòng ban và kỹ thuật | Department, Inventory, WorkOrder, Maintenance, Calibration | Người phụ trách module #3 và chủ repository khi đổi schema |
| Dashboard và Web API | Home thống kê, DTO và endpoint | Người phụ trách module #4 và module sở hữu dữ liệu |

TonLeo2863 phụ trách điều phối các file dùng chung gồm solution, cấu hình, routing, EF DbContext, script SQL, layout, tài nguyên chung và workflow. CODEOWNERS yêu cầu review ở cấp repository. Phân công module trong bảng là đầu mối phối hợp review.

## Trước khi mở PR

- Fetch, ghi SHA nền và tạo nhánh theo phạm vi. Rà `git status`, diff, worktree và tệp ignored có liên quan. Giữ thay đổi của người khác.
- Xử lý quy tắc chung ở controller/helper dùng chung. Dùng ViewModel hoặc danh sách trường bind. Xác minh vai trò, phạm vi đối tượng và CSRF ở server.
- Dùng tiếng Anh cho tên định danh, tiếng Việt có dấu UTF-8 cho chú thích và mô tả công việc. XML Documentation ghi điều kiện nghiệp vụ, đầu vào, đầu ra, khóa và xử lý lỗi ở điểm thay đổi quan trọng.
- Restore theo `packages.config`. Build Debug, Release và Razor. Ghi bằng chứng IIS, SQL hoặc trình duyệt theo phạm vi. Nghiệm thu thủ công là kết quả riêng với kiểm thử tự động.
- Chia commit theo nguyên nhân và module. Tiêu đề theo Conventional Commits, nội dung tiếng Việt. Commit quan trọng ghi nguyên nhân, file, ảnh hưởng SQL, ca thử và giới hạn. Cập nhật changelog/tiến độ trước khi push.
- PR rà cả quyền ghi, schema cũ, dữ liệu hiện có và hồi quy. Đổi giao diện kèm ảnh đã loại dữ liệu nhạy cảm. Chỉ báo CI PASS sau khi xem workflow thực thi.
