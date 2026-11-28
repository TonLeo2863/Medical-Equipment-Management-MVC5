# Hướng dẫn làm việc trong repository

Đọc [quy chuẩn phát triển](Docs/DEVELOPMENT_RULES.md), [nghiệp vụ](Docs/BUSINESS_RULES.md), [database](Docs/DATABASE_RULES.md) và [tiến độ](Docs/FINAL_PROGRESS.md) trước khi thay đổi. Giữ kiến trúc ASP.NET MVC 5, .NET Framework 4.8, EF6, Razor và Web API 2.

- Kiểm tra Git và thay đổi cục bộ trước khi tạo nhánh. Giữ file ngoài phạm vi, bí mật và backup ngoài commit.
- Dùng định danh tiếng Anh. Viết chú thích có ý nghĩa và XML Documentation bằng tiếng Việt có dấu tại các điểm xử lý nghiệp vụ. Giữ UTF-8 và nguyên vẹn mã thư viện.
- Xác minh quyền phía server và phạm vi bản ghi ở mọi điểm ghi. Giữ model binding và CSRF khi sửa giao diện.
- Ghi dữ liệu QA chỉ trên BetaTest theo quy tắc database. Bảo toàn tài khoản hiện hữu và chỉ dọn fixture của chính lượt chạy.
- Báo cáo PASS, FAIL, BLOCKED, NOT RUN theo bằng chứng. Cập nhật changelog và tiến độ, commit tiếng Việt theo Conventional Commits và chỉ push nhánh làm việc đã được giao.
