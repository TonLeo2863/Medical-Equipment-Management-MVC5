# Quy tắc database

1. Chỉ ghi và kiểm thử trên `localhost\SQLEXPRESS`, `MedicalEquipmentDB_BetaTest`. Kiểm tra connection string của IIS và `SELECT DB_NAME()` trước DDL hoặc thao tác ghi quan trọng.
2. Giữ database gốc, database môn khác, tài khoản QA và hash hiện hữu. Dùng namespace fixture riêng, khóa điều kiện dọn theo ID và mã của chính lượt chạy.
3. Backup có checksum và chạy `RESTORE VERIFYONLY` trước migration hoặc thao tác có nguy cơ mất dữ liệu. Giữ tệp backup ngoài repository.
4. Migration dùng transaction, `XACT_ABORT`, guard database, kiểm tra tồn tại và khóa ngoại được tin cậy. Dùng script riêng và đối chiếu mapping EF sau khi chạy.
5. Đọc checkpoint và schema trước lần tiếp tục. Chỉ chạy migration khi cần. Đối chiếu số dư fixture và transaction khi kết thúc.
6. Các script đối chiếu trong TestData chỉ đọc. Chạy bằng quyền đọc khi có thể. Các tệp log và ảnh dùng dữ liệu tổng hợp hoặc fixture, giữ mật khẩu, hash, Cookie, token và cấu hình nhạy cảm ngoài Git.
