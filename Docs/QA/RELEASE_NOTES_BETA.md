# Ghi chú Beta

**Beta candidate / not ready**. Nhánh kết hợp PR #5 kho, phòng ban, điều chuyển, bảo trì và phiếu sửa chữa, PR #6 tài khoản, layout, menu và trang lỗi, PR #7 danh mục, tài sản, Excel và QR/Barcode. SHA nguồn và các quyết định hợp nhất có trong PR_REVIEW.md và MERGE_RESOLUTION.md.

Beta đăng ký Web API 2, dùng EquipmentAssets và view model dạng JSON, kiểm tra session, quyền và token cho thao tác ghi API. Ứng dụng đích .NET Framework 4.8. Đăng nhập chỉ chấp nhận PBKDF2-SHA256. Các tài khoản demo cũ cần đặt lại mật khẩu bằng giá trị dạng `pbkdf2-sha256-v1$<salt base64>$<key base64>` với salt ngẫu nhiên 16 byte, 600.000 vòng, khóa 32 byte. QA tạo tài khoản riêng trên cơ sở dữ liệu thử nghiệm.

Dùng Visual Studio 2022 Desktop/Web build tools và .NET Framework 4.8 Developer Pack. Nếu đường dẫn worktree quá dài, sao chép solution và thư mục dự án sang đường dẫn ngắn trước khi restore. Cấu hình mặc định của Beta trỏ đến SQL Express cục bộ, database `MedicalEquipmentDB_BetaTest`. Khi chạy QA, chỉnh bản Web.config tạm để dùng tài khoản SQL giới hạn cho database thử nghiệm. Xem BETA_TEST_REPORT.md để biết cách tạo bản sao script và xác minh đích.

Ưu tiên sửa seed chạy lặp, xác nhận quyền ghi theo vai trò và hoàn tất kiểm thử nghiệp vụ cùng API. Các PR nguồn vẫn mở về develop theo lần kiểm tra GitHub. Việc tích hợp ngược cần quyết định của chủ repository sau QA.
