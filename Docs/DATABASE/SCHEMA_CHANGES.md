# Thay đổi schema cho nhánh phát hành

Script `MedicalEquipmentManagement/Database/03_FinalCandidate_Workflow.sql` chỉ chạy trên `MedicalEquipmentDB_BetaTest`. Script dùng `XACT_ABORT`, transaction, kiểm tra database đích và kiểm tra tồn tại trước khi thêm cột hoặc khóa ngoại.

| Bảng | Thay đổi |
|---|---|
| WorkOrders | Thêm `AssignedToUserId`, `ResolvedByUserId` nullable trỏ Users, `ReporterVerified` mặc định 0, `Resolution` và `TechnicalPassed` nullable. Cột SQL `ReportedBy` tiếp tục bắt buộc và ánh xạ vào `ReportedByUserId`. |
| WorkOrders | Chuẩn hóa trạng thái cũ `Đã hoàn thành` thành `Đã xong`. Giữ giá trị ReportedBy cũ và đánh dấu dữ liệu chưa xác minh. Người báo mới lấy từ Session. |
| MaintenanceRecords | Ánh xạ lại `TicketId` hiện có và thêm khóa ngoại WorkOrders, thêm `PerformedByUserId` nullable trỏ Users. Bản ghi cũ chưa có kỹ thuật viên giữ giá trị null. |

Trước migration, sao lưu BetaTest và chạy `RESTORE VERIFYONLY`. Trên máy QA, file `MedicalEquipmentDB_BetaTest_final_candidate_20261128103152.bak` qua xác minh checksum. Hai lượt migration exit 0. Hai phiếu hiện có được bảo toàn, các khóa ngoại mới đều được tin cậy. Khâu kiểm tra trước migration tìm thấy 0 phiếu bảo trì sai liên kết. Chưa thử khôi phục vật lý sang database riêng.

Để chạy lại trên môi trường được phép, dùng:

```powershell
sqlcmd -S 'localhost\SQLEXPRESS' -E -d MedicalEquipmentDB_BetaTest -b -f 65001 -i MedicalEquipmentManagement/Database/03_FinalCandidate_Workflow.sql
```

Kiểm tra `DB_NAME()` và quyền truy cập trước khi áp dụng. Nếu có sai khác schema hoặc dữ liệu khiến lệnh thất bại, lấy thông báo từ SQL Server và xác định nguyên nhân trước khi thử lại. Tiếp tục giữ backup ở ngoài Git.