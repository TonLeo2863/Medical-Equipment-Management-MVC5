-- Chỉ đọc nguồn dữ liệu Dashboard trên BetaTest. Đặt tham số giống bộ lọc đang mở.
SET NOCOUNT ON;
IF DB_NAME() <> N'MedicalEquipmentDB_BetaTest'
    THROW 51000, N'Chỉ chạy đối chiếu trên MedicalEquipmentDB_BetaTest.', 1;

DECLARE @DepartmentId int = NULL, @Year int = YEAR(GETDATE()), @Days int = 30;
DECLARE @Deadline date = DATEADD(day, @Days, CAST(GETDATE() AS date));
DECLARE @From date = DATEFROMPARTS(@Year, 1, 1), @To date = DATEFROMPARTS(@Year + 1, 1, 1);

SELECT Status, COUNT(*) AS Total
FROM dbo.EquipmentAssets
WHERE IsDeleted = 0 AND (@DepartmentId IS NULL OR DepartmentId = @DepartmentId)
GROUP BY Status;

SELECT ISNULL(d.Name, N'Chưa phân bổ') AS DepartmentName, COUNT(*) AS Total
FROM dbo.EquipmentAssets a
LEFT JOIN dbo.Departments d ON a.DepartmentId = d.DepartmentId
WHERE a.IsDeleted = 0 AND (@DepartmentId IS NULL OR a.DepartmentId = @DepartmentId)
GROUP BY d.DepartmentId, d.Name ORDER BY DepartmentName;

-- Tổng phiếu dành cho ADMIN/MANAGER. STAFF và ENGINEER còn lọc theo người báo hoặc người được giao.
SELECT COUNT(*) AS OpenOrders
FROM dbo.WorkOrders w JOIN dbo.EquipmentAssets a ON a.AssetId = w.AssetId
WHERE a.IsDeleted = 0 AND w.Status NOT IN (N'Đã xong', N'Đã hủy')
    AND (@DepartmentId IS NULL OR a.DepartmentId = @DepartmentId);

SELECT MONTH(m.MaintenanceDate) AS [Month], SUM(ISNULL(m.Cost, 0)) AS Cost
FROM dbo.MaintenanceRecords m JOIN dbo.EquipmentAssets a ON a.AssetId = m.AssetId
WHERE m.MaintenanceDate >= @From AND m.MaintenanceDate < @To
    AND (@DepartmentId IS NULL OR a.DepartmentId = @DepartmentId)
GROUP BY MONTH(m.MaintenanceDate) ORDER BY [Month];

-- Ngày rồi khóa chính xác định bản ghi mới nhất, kể cả khi bản ghi mới nhất không có lịch kế tiếp.
WITH LatestMaintenance AS (
    SELECT AssetId, NextMaintenanceDate, ROW_NUMBER() OVER (PARTITION BY AssetId ORDER BY MaintenanceDate DESC, MaintenanceId DESC) AS Position
    FROM dbo.MaintenanceRecords
), LatestCalibration AS (
    SELECT AssetId, NextCalibrationDate, ROW_NUMBER() OVER (PARTITION BY AssetId ORDER BY CalibrationDate DESC, CalibrationId DESC) AS Position
    FROM dbo.CalibrationLogs
)
SELECT a.AssetCode, m.NextMaintenanceDate, c.NextCalibrationDate
FROM dbo.EquipmentAssets a
LEFT JOIN LatestMaintenance m ON a.AssetId = m.AssetId AND m.Position = 1
LEFT JOIN LatestCalibration c ON a.AssetId = c.AssetId AND c.Position = 1
WHERE a.IsDeleted = 0 AND a.Status <> N'Đã thanh lý'
    AND (@DepartmentId IS NULL OR a.DepartmentId = @DepartmentId)
    AND (m.NextMaintenanceDate <= @Deadline OR c.NextCalibrationDate <= @Deadline)
ORDER BY a.AssetCode;
