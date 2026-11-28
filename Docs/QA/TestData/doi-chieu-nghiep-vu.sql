-- Dùng mã fixture của người kiểm thử. Truy vấn tài khoản chỉ trả trạng thái và loại hash.
SET NOCOUNT ON;
IF DB_NAME() <> N'MedicalEquipmentDB_BetaTest'
    THROW 51000, N'Chỉ chạy đối chiếu trên MedicalEquipmentDB_BetaTest.', 1;

DECLARE @Username nvarchar(50) = N'RCUI-QA-01';
DECLARE @AssetCode nvarchar(50) = N'RCUI-VALID-01';

SELECT u.UserId, u.Username, u.IsActive, r.RoleCode,
    CASE WHEN u.PasswordHash LIKE 'pbkdf2-sha256-v1$%' THEN N'PBKDF2 v1' ELSE N'Định dạng khác' END AS HashFormat
FROM dbo.Users u JOIN dbo.Roles r ON r.RoleId = u.RoleId
WHERE u.Username = @Username;

DECLARE @AssetId int = (SELECT AssetId FROM dbo.EquipmentAssets WHERE AssetCode = @AssetCode);

SELECT a.AssetId, a.AssetCode, a.SerialNumber, a.DepartmentId, d.Name AS DepartmentName, a.Status, a.IsDeleted
FROM dbo.EquipmentAssets a LEFT JOIN dbo.Departments d ON d.DepartmentId = a.DepartmentId
WHERE a.AssetId = @AssetId;

SELECT TransferId, FromDepartmentId, ToDepartmentId, RequestedBy, TransferDate
FROM dbo.AssetTransfers WHERE AssetId = @AssetId ORDER BY TransferId;

SELECT TransactionId, DepartmentId, TransactionType, Quantity, TransactionDate
FROM dbo.InventoryTransactions WHERE EquipmentId = @AssetId ORDER BY TransactionId;

SELECT TicketId, TicketCode, ReportedBy, ReporterVerified, AssignedToUserId, ResolvedByUserId, Status
FROM dbo.WorkOrders WHERE AssetId = @AssetId ORDER BY TicketId;

SELECT MaintenanceId, TicketId, PerformedByUserId, MaintenanceDate, NextMaintenanceDate, ResultStatus
FROM dbo.MaintenanceRecords WHERE AssetId = @AssetId ORDER BY MaintenanceId;

SELECT CalibrationId, CalibrationDate, NextCalibrationDate, Result
FROM dbo.CalibrationLogs WHERE AssetId = @AssetId ORDER BY CalibrationId;
