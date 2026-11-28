# Kinh nghiệm tích hợp

- PR #5 đưa điều chuyển, bảo trì và sửa chữa vào cùng nhánh. Khi nhiều module ghi cùng AssetId, thống nhất thứ tự khóa và transaction. Đối chiếu cột ReportedBy, người phân công và trường người thực hiện giữa SQL và EF. [PR #5](https://github.com/TonLeo2863/Medical-Equipment-Management-MVC5/pull/5)
- PR #6 mang theo thay đổi tài khoản và layout. Review phải kiểm tra cả auth trên URL trực tiếp, CSRF, hash cũ và model binding qua trình duyệt. Phân quyền không phụ thuộc menu. [PR #6](https://github.com/TonLeo2863/Medical-Equipment-Management-MVC5/pull/6)
- PR #7 thay đổi thực thể dùng chung và danh mục. Khi chuyển từ MVC sang Web API, cần kiểm thử cùng quy tắc xóa, khóa ngoại, chuẩn hóa mã và giữ lịch sử. Dữ liệu mẫu Excel nên tách từng loại lỗi. [PR #7](https://github.com/TonLeo2863/Medical-Equipment-Management-MVC5/pull/7)
- BETA-005 cho thấy compile không bắt lỗi SQL NOT NULL khi tên thuộc tính EF mang ý nghĩa khác cột. Kiểm POST thật, kiểm tra SQL, ghi InnerException ở môi trường QA.
- RC-005–007 cho thấy mật khẩu ngẫu nhiên chỉ gồm hex không bao phủ các ký tự giống HTML. Kiểm bằng form thật và xác minh cả mật khẩu cũ/mới.
- Các ca lỗi trong harness cần tách khỏi lỗi nghiệp vụ. Đọc mảng JSON theo đúng hành vi PowerShell, dùng tên cột SQL thực tế và giải mã HTML trước khi so thông báo tiếng Việt.
