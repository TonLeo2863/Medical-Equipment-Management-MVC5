-- SQL Server 2016+
IF DB_ID(N'MedicalEquipmentDB') IS NULL
    CREATE DATABASE MedicalEquipmentDB;
GO
USE MedicalEquipmentDB;
GO

-- ==========================================
-- PHẦN 1: TẠO CẤU TRÚC BẢNG (SCHEMA)
-- ==========================================

-- 1. BẢNG PHÂN QUYỀN (Role-Based Access Control)
IF OBJECT_ID('dbo.Roles','U') IS NULL
CREATE TABLE Roles(
    RoleId INT IDENTITY PRIMARY KEY,
    RoleCode NVARCHAR(20) NOT NULL UNIQUE,
    RoleName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(250) NULL
);

-- 2. BẢNG NGƯỜI DÙNG
IF OBJECT_ID('dbo.Users','U') IS NULL
CREATE TABLE Users(
    UserId INT IDENTITY PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    FullName NVARCHAR(150) NOT NULL,
    RoleId INT NOT NULL,
    Email NVARCHAR(150) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Users_Roles FOREIGN KEY(RoleId) REFERENCES Roles(RoleId)
);

-- 3. BẢNG DANH MỤC PHÒNG BAN
IF OBJECT_ID('dbo.Departments','U') IS NULL
CREATE TABLE Departments(
    DepartmentId INT IDENTITY PRIMARY KEY,
    DepartmentCode NVARCHAR(20) NOT NULL UNIQUE,
    Name NVARCHAR(150) NOT NULL,
    Location NVARCHAR(250) NULL,
    IsActive BIT NOT NULL DEFAULT 1
);

-- 4. BẢNG NHÀ CUNG CẤP & HÃNG SẢN XUẤT
IF OBJECT_ID('dbo.Suppliers','U') IS NULL
CREATE TABLE Suppliers(
    SupplierId INT IDENTITY PRIMARY KEY,
    Name NVARCHAR(150) NOT NULL,
    SupplierType NVARCHAR(50) NOT NULL DEFAULT N'Vendor',
    Phone NVARCHAR(30) NULL,
    Email NVARCHAR(150) NULL,
    IsActive BIT NOT NULL DEFAULT 1
);

-- 5. BẢNG DANH MỤC LOẠI THIẾT BỊ
IF OBJECT_ID('dbo.EquipmentCategories','U') IS NULL
CREATE TABLE EquipmentCategories(
    CategoryId INT IDENTITY PRIMARY KEY,
    CategoryCode NVARCHAR(20) NOT NULL UNIQUE,
    Name NVARCHAR(150) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1
);

-- 6. BẢNG DÒNG THIẾT BỊ (Model)
IF OBJECT_ID('dbo.EquipmentModels','U') IS NULL
CREATE TABLE EquipmentModels(
    ModelId INT IDENTITY PRIMARY KEY,
    ModelCode NVARCHAR(50) NOT NULL UNIQUE,
    Name NVARCHAR(200) NOT NULL,
    CategoryId INT NOT NULL,
    ManufacturerId INT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_Models_Categories FOREIGN KEY(CategoryId) REFERENCES EquipmentCategories(CategoryId),
    CONSTRAINT FK_Models_Manufacturers FOREIGN KEY(ManufacturerId) REFERENCES Suppliers(SupplierId)
);

-- 7. BẢNG HỢP ĐỒNG BẢO TRÌ/MUA SẮM
IF OBJECT_ID('dbo.Contracts','U') IS NULL
CREATE TABLE Contracts(
    ContractId INT IDENTITY PRIMARY KEY,
    ContractNumber NVARCHAR(50) NOT NULL UNIQUE,
    SupplierId INT NOT NULL,
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    ContractValue DECIMAL(18,2) NULL,
    ContractType NVARCHAR(50) NOT NULL, -- Mua sắm, Bảo trì toàn diện, Bảo trì bán phần
    CONSTRAINT FK_Contracts_Suppliers FOREIGN KEY(SupplierId) REFERENCES Suppliers(SupplierId)
);

-- 8. BẢNG THIẾT BỊ VẬT LÝ (Asset)
IF OBJECT_ID('dbo.EquipmentAssets','U') IS NULL
CREATE TABLE EquipmentAssets(
    AssetId INT IDENTITY PRIMARY KEY,
    AssetCode NVARCHAR(50) NOT NULL UNIQUE,
    SerialNumber NVARCHAR(100) NOT NULL,
    ModelId INT NOT NULL,
    DepartmentId INT NULL,
    ContractId INT NULL,
    PurchaseDate DATE NULL,
    PurchasePrice DECIMAL(18,2) NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT N'Đang hoạt động',
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_Assets_Models FOREIGN KEY(ModelId) REFERENCES EquipmentModels(ModelId),
    CONSTRAINT FK_Assets_Departments FOREIGN KEY(DepartmentId) REFERENCES Departments(DepartmentId),
    CONSTRAINT FK_Assets_Contracts FOREIGN KEY(ContractId) REFERENCES Contracts(ContractId)
);

-- 9. BẢNG LỊCH SỬ BẢO TRÌ (Sửa chữa, Bảo dưỡng)
IF OBJECT_ID('dbo.MaintenanceRecords','U') IS NULL
CREATE TABLE MaintenanceRecords(
    MaintenanceId INT IDENTITY PRIMARY KEY,
    AssetId INT NOT NULL,
    MaintenanceType NVARCHAR(50) NOT NULL,
    MaintenanceDate DATE NOT NULL,
    Cost DECIMAL(18,2) NULL,
    ResultStatus NVARCHAR(100) NOT NULL,
    NextMaintenanceDate DATE NULL,
    Description NVARCHAR(500) NULL,
    TicketId INT NULL,
    CONSTRAINT FK_Maintenance_Assets FOREIGN KEY(AssetId) REFERENCES EquipmentAssets(AssetId)
);

-- 10. BẢNG NHẬT KÝ KIỂM ĐỊNH (Calibration - Bắt buộc cho thiết bị y tế)
IF OBJECT_ID('dbo.CalibrationLogs','U') IS NULL
CREATE TABLE CalibrationLogs(
    CalibrationId INT IDENTITY PRIMARY KEY,
    AssetId INT NOT NULL,
    CalibrationDate DATE NOT NULL,
    NextCalibrationDate DATE NOT NULL,
    PerformedBy NVARCHAR(150) NOT NULL, -- Đơn vị kiểm định (VD: TT Kiểm định VN)
    CertificateNumber NVARCHAR(100) NULL,
    Result NVARCHAR(50) NOT NULL, -- Đạt / Không đạt
    CONSTRAINT FK_Calibration_Assets FOREIGN KEY(AssetId) REFERENCES EquipmentAssets(AssetId)
);

-- 11. BẢNG PHIẾU BÁO HỎNG / SỰ CỐ (Work Orders)
-- Dùng để các Khoa/Phòng báo cáo khi thiết bị gặp sự cố
IF OBJECT_ID('dbo.WorkOrders','U') IS NULL
CREATE TABLE WorkOrders(
    TicketId INT IDENTITY PRIMARY KEY,
    TicketCode NVARCHAR(50) NOT NULL UNIQUE,
    AssetId INT NOT NULL,
    ReportedBy INT NOT NULL, -- Người báo hỏng (UserId)
    ReportedDate DATETIME NOT NULL DEFAULT GETDATE(),
    IssueDescription NVARCHAR(500) NOT NULL,
    UrgencyLevel NVARCHAR(20) NOT NULL DEFAULT N'Bình thường', -- Thấp, Bình thường, Cao, Cấp cứu
    Status NVARCHAR(50) NOT NULL DEFAULT N'Mới tạo', -- Mới tạo, Đang xử lý, Đã hoàn thành, Hủy
    ResolvedDate DATETIME NULL,
    CONSTRAINT FK_WorkOrders_Assets FOREIGN KEY(AssetId) REFERENCES EquipmentAssets(AssetId),
    CONSTRAINT FK_WorkOrders_Users FOREIGN KEY(ReportedBy) REFERENCES Users(UserId)
);

IF COL_LENGTH('dbo.MaintenanceRecords', 'NextMaintenanceDate') IS NULL
BEGIN
    ALTER TABLE MaintenanceRecords ADD NextMaintenanceDate DATE NULL;
END
IF COL_LENGTH('dbo.MaintenanceRecords', 'Description') IS NULL
BEGIN
    ALTER TABLE MaintenanceRecords ADD Description NVARCHAR(500) NULL;
END
IF COL_LENGTH('dbo.MaintenanceRecords', 'TicketId') IS NULL
BEGIN
    ALTER TABLE MaintenanceRecords ADD TicketId INT NULL;
END

-- BẢNG NHẬT KÝ GIAO DỊCH KHO (Inventory Transactions)
IF OBJECT_ID('dbo.InventoryTransactions','U') IS NULL
CREATE TABLE InventoryTransactions(
    TransactionId INT IDENTITY PRIMARY KEY,
    EquipmentId INT NOT NULL,
    DepartmentId INT NULL,
    TransactionType NVARCHAR(50) NOT NULL,
    Quantity INT NOT NULL DEFAULT 1,
    TransactionDate DATETIME NOT NULL DEFAULT GETDATE(),
    Note NVARCHAR(500) NULL
);


-- 12. BẢNG LỊCH SỬ LUÂN CHUYỂN THIẾT BỊ (Asset Transfers)
IF OBJECT_ID('dbo.AssetTransfers','U') IS NULL
CREATE TABLE AssetTransfers(
    TransferId INT IDENTITY PRIMARY KEY,
    AssetId INT NOT NULL,
    FromDepartmentId INT NOT NULL,
    ToDepartmentId INT NOT NULL,
    TransferDate DATETIME NOT NULL DEFAULT GETDATE(),
    RequestedBy INT NOT NULL,
    ApprovedBy INT NULL,
    Reason NVARCHAR(250) NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT N'Chờ duyệt', -- Chờ duyệt, Đã chuyển, Từ chối
    CONSTRAINT FK_Transfers_Assets FOREIGN KEY(AssetId) REFERENCES EquipmentAssets(AssetId),
    CONSTRAINT FK_Transfers_FromDept FOREIGN KEY(FromDepartmentId) REFERENCES Departments(DepartmentId),
    CONSTRAINT FK_Transfers_ToDept FOREIGN KEY(ToDepartmentId) REFERENCES Departments(DepartmentId),
    CONSTRAINT FK_Transfers_ReqUser FOREIGN KEY(RequestedBy) REFERENCES Users(UserId)
);

-- 13. BẢNG DANH MỤC LINH KIỆN / VẬT TƯ (Spare Parts)
IF OBJECT_ID('dbo.SpareParts','U') IS NULL
CREATE TABLE SpareParts(
    PartId INT IDENTITY PRIMARY KEY,
    PartCode NVARCHAR(50) NOT NULL UNIQUE,
    PartName NVARCHAR(200) NOT NULL,
    ModelId INT NULL, -- Linh kiện có thể dùng chung (NULL) hoặc chỉ định cho Model cụ thể
    StockQuantity INT NOT NULL DEFAULT 0,
    Unit NVARCHAR(20) NOT NULL, -- Cái, Sợi, Lọ, Bộ...
    UnitPrice DECIMAL(18,2) NOT NULL DEFAULT 0,
    CONSTRAINT FK_SpareParts_Models FOREIGN KEY(ModelId) REFERENCES EquipmentModels(ModelId)
);

-- 14. BẢNG CHI TIẾT SỬ DỤNG LINH KIỆN TRONG BẢO TRÌ (Maintenance Spare Parts)
IF OBJECT_ID('dbo.Maintenance_SpareParts','U') IS NULL
CREATE TABLE Maintenance_SpareParts(
    MaintenanceId INT NOT NULL,
    PartId INT NOT NULL,
    QuantityUsed INT NOT NULL DEFAULT 1,
    Note NVARCHAR(200) NULL,
    PRIMARY KEY(MaintenanceId, PartId),
    CONSTRAINT FK_MaintParts_Maintenance FOREIGN KEY(MaintenanceId) REFERENCES MaintenanceRecords(MaintenanceId),
    CONSTRAINT FK_MaintParts_Parts FOREIGN KEY(PartId) REFERENCES SpareParts(PartId)
);

-- 15. BẢNG NHẬT KÝ HOẠT ĐỘNG / CÔNG SUẤT SỬ DỤNG (Usage Logs)
-- Rất quan trọng đối với máy CT/MRI để theo dõi tuổi thọ bóng X-Quang (Tube count)
IF OBJECT_ID('dbo.UsageLogs','U') IS NULL
CREATE TABLE UsageLogs(
    LogId INT IDENTITY PRIMARY KEY,
    AssetId INT NOT NULL,
    LogDate DATE NOT NULL,
    UsageCount INT NOT NULL, -- Số ca chụp, hoặc số giờ hoạt động
    Unit NVARCHAR(50) NOT NULL, -- Ca (Scans), Giờ (Hours)
    RecordedBy INT NOT NULL,
    CONSTRAINT FK_UsageLogs_Assets FOREIGN KEY(AssetId) REFERENCES EquipmentAssets(AssetId),
    CONSTRAINT FK_UsageLogs_Users FOREIGN KEY(RecordedBy) REFERENCES Users(UserId)
);
-- ==========================================
-- PHẦN 2: DỮ LIỆU MẪU (SAMPLE DATA)
-- ==========================================

-- Preserve existing demo and application data. Serialize concurrent seed executions.
SET XACT_ABORT ON;
BEGIN TRANSACTION;
DECLARE @SeedLock INT;
EXEC @SeedLock = sys.sp_getapplock
    @Resource = N'MedicalEquipmentDemoSeed', @LockMode = 'Exclusive', @LockTimeout = 15000;
IF @SeedLock < 0
    THROW 51001, 'Could not acquire the demo seed lock.', 1;

INSERT INTO dbo.Roles (RoleCode, RoleName, Description)
SELECT sample.RoleCode, sample.RoleName, sample.Description
FROM (VALUES
    ('ADMIN', N'Quản trị hệ thống', N'Toàn quyền'),
    ('MANAGER', N'Trưởng khoa/Phòng vật tư', N'Quản lý thiết bị và duyệt luân chuyển'),
    ('ENGINEER', N'Kỹ sư y sinh', N'Cập nhật bảo trì, sửa chữa'),
    ('STAFF', N'Nhân viên y tế', N'Xem thiết bị tại khoa')
) AS sample (RoleCode, RoleName, Description)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Roles AS existing
    WHERE existing.RoleCode = sample.RoleCode
);

INSERT INTO dbo.Users (Username, PasswordHash, FullName, RoleId, Email)
SELECT sample.Username, sample.PasswordHash, sample.FullName, sample.RoleId, sample.Email
FROM (VALUES
    ('admin', 'hash_123', N'Quản trị viên', (SELECT RoleId FROM dbo.Roles WHERE RoleCode = 'ADMIN'), 'admin@hospital.vn'),
    ('truongkhoa', 'hash_123', N'BS. Nguyễn Văn A', (SELECT RoleId FROM dbo.Roles WHERE RoleCode = 'MANAGER'), 'khoa_a@hospital.vn'),
    ('kysu1', 'hash_123', N'KS. Trần Văn B', (SELECT RoleId FROM dbo.Roles WHERE RoleCode = 'ENGINEER'), 'kysu@hospital.vn')
) AS sample (Username, PasswordHash, FullName, RoleId, Email)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Users AS existing
    WHERE existing.Username = sample.Username
);

INSERT INTO dbo.Users (Username, PasswordHash, FullName, RoleId, Email)
SELECT sample.Username, sample.PasswordHash, sample.FullName, sample.RoleId, sample.Email
FROM (VALUES
    ('bs_cuong', 'hash_123', N'BS. Lê Văn Cường', (SELECT RoleId FROM dbo.Roles WHERE RoleCode = 'STAFF'), 'bs_cuong@hospital.vn')
) AS sample (Username, PasswordHash, FullName, RoleId, Email)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Users AS existing
    WHERE existing.Username = sample.Username
);

INSERT INTO dbo.Departments (DepartmentCode, Name, Location)
SELECT sample.DepartmentCode, sample.Name, sample.Location
FROM (VALUES
    ('KCC', N'Khoa Cấp Cứu', N'Tầng 1 - Tòa A'),
    ('KCDHA', N'Khoa Chẩn đoán hình ảnh', N'Tầng 1 - Tòa B'),
    ('KXN', N'Khoa Xét nghiệm', N'Tầng 2 - Tòa B')
) AS sample (DepartmentCode, Name, Location)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Departments AS existing
    WHERE existing.DepartmentCode = sample.DepartmentCode
);

INSERT INTO dbo.Suppliers (Name, SupplierType, Phone, Email)
SELECT sample.Name, sample.SupplierType, sample.Phone, sample.Email
FROM (VALUES
    ('Philips Healthcare', 'Manufacturer', '18001111', 'contact@philips.com'),
    ('Siemens Healthineers', 'Manufacturer', '18002222', 'contact@siemens.com'),
    (N'Công ty TNHH TBYT Phương Đông', 'Vendor', '02833334444', 'sales@phuongdong.vn')
) AS sample (Name, SupplierType, Phone, Email)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Suppliers AS existing
    WHERE existing.Email = sample.Email
);

INSERT INTO dbo.EquipmentCategories (CategoryCode, Name)
SELECT sample.CategoryCode, sample.Name
FROM (VALUES
    ('XQ', N'Hệ thống X-Quang'),
    ('SA', N'Máy Siêu Âm'),
    ('MON', N'Monitor theo dõi bệnh nhân')
) AS sample (CategoryCode, Name)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.EquipmentCategories AS existing
    WHERE existing.CategoryCode = sample.CategoryCode
);

INSERT INTO dbo.EquipmentModels (ModelCode, Name, CategoryId, ManufacturerId)
SELECT sample.ModelCode, sample.Name, sample.CategoryId, sample.ManufacturerId
FROM (VALUES
    ('PH-EPIQ7', 'Philips EPIQ 7 Ultrasound', (SELECT CategoryId FROM dbo.EquipmentCategories WHERE CategoryCode = 'SA'), (SELECT MIN(SupplierId) FROM dbo.Suppliers WHERE Email = 'contact@philips.com')),
    ('SM-SOMATOM', 'Siemens SOMATOM CT Scanner', (SELECT CategoryId FROM dbo.EquipmentCategories WHERE CategoryCode = 'XQ'), (SELECT MIN(SupplierId) FROM dbo.Suppliers WHERE Email = 'contact@siemens.com')),
    ('PH-MX40', 'Philips IntelliVue MX40', (SELECT CategoryId FROM dbo.EquipmentCategories WHERE CategoryCode = 'MON'), (SELECT MIN(SupplierId) FROM dbo.Suppliers WHERE Email = 'contact@philips.com'))
) AS sample (ModelCode, Name, CategoryId, ManufacturerId)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.EquipmentModels AS existing
    WHERE existing.ModelCode = sample.ModelCode
);

INSERT INTO dbo.Contracts (ContractNumber, SupplierId, StartDate, EndDate, ContractType)
SELECT sample.ContractNumber, sample.SupplierId, sample.StartDate, sample.EndDate, sample.ContractType
FROM (VALUES
    ('HD-2023-001', (SELECT MIN(SupplierId) FROM dbo.Suppliers WHERE Email = 'sales@phuongdong.vn'), '2023-01-01', '2026-12-31', N'Bảo trì toàn diện')
) AS sample (ContractNumber, SupplierId, StartDate, EndDate, ContractType)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Contracts AS existing
    WHERE existing.ContractNumber = sample.ContractNumber
);

INSERT INTO dbo.EquipmentAssets (AssetCode, SerialNumber, ModelId, DepartmentId, ContractId, PurchaseDate, PurchasePrice, Status)
SELECT sample.AssetCode, sample.SerialNumber, sample.ModelId, sample.DepartmentId, sample.ContractId, sample.PurchaseDate, sample.PurchasePrice, sample.Status
FROM (VALUES
    ('SA-001', 'SN-PH7-9901', (SELECT ModelId FROM dbo.EquipmentModels WHERE ModelCode = 'PH-EPIQ7'), (SELECT DepartmentId FROM dbo.Departments WHERE DepartmentCode = 'KCDHA'), (SELECT ContractId FROM dbo.Contracts WHERE ContractNumber = 'HD-2023-001'), '2023-02-15', 1500000000, N'Đang hoạt động'),
    ('SA-002', 'SN-PH7-9902', (SELECT ModelId FROM dbo.EquipmentModels WHERE ModelCode = 'PH-EPIQ7'), (SELECT DepartmentId FROM dbo.Departments WHERE DepartmentCode = 'KCC'), (SELECT ContractId FROM dbo.Contracts WHERE ContractNumber = 'HD-2023-001'), '2023-02-15', 1500000000, N'Đang hoạt động'),
    ('CT-001', 'SN-SM-CT881', (SELECT ModelId FROM dbo.EquipmentModels WHERE ModelCode = 'SM-SOMATOM'), (SELECT DepartmentId FROM dbo.Departments WHERE DepartmentCode = 'KCDHA'), (SELECT ContractId FROM dbo.Contracts WHERE ContractNumber = 'HD-2023-001'), '2022-10-10', 8500000000, N'Đang hoạt động'),
    ('MON-001', 'SN-MX40-111', (SELECT ModelId FROM dbo.EquipmentModels WHERE ModelCode = 'PH-MX40'), (SELECT DepartmentId FROM dbo.Departments WHERE DepartmentCode = 'KCC'), NULL, '2024-01-05', 120000000, N'Đang bảo trì')
) AS sample (AssetCode, SerialNumber, ModelId, DepartmentId, ContractId, PurchaseDate, PurchasePrice, Status)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.EquipmentAssets AS existing
    WHERE existing.AssetCode = sample.AssetCode
);

INSERT INTO dbo.WorkOrders (TicketCode, AssetId, ReportedBy, ReportedDate, IssueDescription, UrgencyLevel, Status)
SELECT sample.TicketCode, sample.AssetId, sample.ReportedBy, sample.ReportedDate, sample.IssueDescription, sample.UrgencyLevel, sample.Status
FROM (VALUES
    ('WO-2024-001', (SELECT AssetId FROM dbo.EquipmentAssets WHERE AssetCode = 'MON-001'), (SELECT UserId FROM dbo.Users WHERE Username = 'bs_cuong'), '2024-03-09 08:30:00', N'Máy Monitor mất tín hiệu đo SpO2, màn hình thỉnh thoảng chớp nháy.', N'Cao', N'Đã hoàn thành')
) AS sample (TicketCode, AssetId, ReportedBy, ReportedDate, IssueDescription, UrgencyLevel, Status)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.WorkOrders AS existing
    WHERE existing.TicketCode = sample.TicketCode
);

INSERT INTO dbo.MaintenanceRecords (AssetId, MaintenanceType, MaintenanceDate, Cost, ResultStatus, TicketId)
SELECT sample.AssetId, sample.MaintenanceType, sample.MaintenanceDate, sample.Cost, sample.ResultStatus, sample.TicketId
FROM (VALUES
    ((SELECT AssetId FROM dbo.EquipmentAssets WHERE AssetCode = 'MON-001'), N'Sửa chữa đột xuất', '2024-03-10', 5500000, N'Đang chờ linh kiện', (SELECT TicketId FROM dbo.WorkOrders WHERE TicketCode = 'WO-2024-001')),
    ((SELECT AssetId FROM dbo.EquipmentAssets WHERE AssetCode = 'SA-001'), N'Bảo dưỡng định kỳ', '2024-02-15', 0, N'Đạt yêu cầu (Thuộc HĐ)', NULL)
) AS sample (AssetId, MaintenanceType, MaintenanceDate, Cost, ResultStatus, TicketId)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.MaintenanceRecords AS existing
    WHERE existing.AssetId = sample.AssetId
      AND existing.MaintenanceType = sample.MaintenanceType
      AND existing.MaintenanceDate = sample.MaintenanceDate
);

INSERT INTO dbo.CalibrationLogs (AssetId, CalibrationDate, NextCalibrationDate, PerformedBy, CertificateNumber, Result)
SELECT sample.AssetId, sample.CalibrationDate, sample.NextCalibrationDate, sample.PerformedBy, sample.CertificateNumber, sample.Result
FROM (VALUES
    ((SELECT AssetId FROM dbo.EquipmentAssets WHERE AssetCode = 'SA-001'), '2023-02-20', '2024-02-20', N'Viện Trang thiết bị và Công trình y tế', 'KD-23-0012', N'Đạt'),
    ((SELECT AssetId FROM dbo.EquipmentAssets WHERE AssetCode = 'CT-001'), '2022-10-15', '2023-10-15', N'Trung tâm Kiểm định An toàn Bức xạ', 'BX-22-0991', N'Đạt')
) AS sample (AssetId, CalibrationDate, NextCalibrationDate, PerformedBy, CertificateNumber, Result)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.CalibrationLogs AS existing
    WHERE existing.AssetId = sample.AssetId
      AND existing.CertificateNumber = sample.CertificateNumber
);

INSERT INTO dbo.SpareParts (PartCode, PartName, ModelId, StockQuantity, Unit, UnitPrice)
SELECT sample.PartCode, sample.PartName, sample.ModelId, sample.StockQuantity, sample.Unit, sample.UnitPrice
FROM (VALUES
    ('SP-SPO2-01', N'Cáp đo SpO2 dùng nhiều lần', (SELECT ModelId FROM dbo.EquipmentModels WHERE ModelCode = 'PH-MX40'), 49, N'Sợi', 1500000),
    ('SP-CT-TUBE', N'Bóng phát tia X cho CT Somatom', (SELECT ModelId FROM dbo.EquipmentModels WHERE ModelCode = 'SM-SOMATOM'), 1, N'Cái', 2500000000),
    ('SP-ULTRA-PR', N'Đầu dò siêu âm Convex', (SELECT ModelId FROM dbo.EquipmentModels WHERE ModelCode = 'PH-EPIQ7'), 3, N'Cái', 120000000)
) AS sample (PartCode, PartName, ModelId, StockQuantity, Unit, UnitPrice)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.SpareParts AS existing
    WHERE existing.PartCode = sample.PartCode
);

INSERT INTO dbo.AssetTransfers (AssetId, FromDepartmentId, ToDepartmentId, RequestedBy, ApprovedBy, Reason, Status, TransferDate)
SELECT sample.AssetId, sample.FromDepartmentId, sample.ToDepartmentId, sample.RequestedBy, sample.ApprovedBy, sample.Reason, sample.Status, sample.TransferDate
FROM (VALUES
    ((SELECT AssetId FROM dbo.EquipmentAssets WHERE AssetCode = 'SA-002'), (SELECT DepartmentId FROM dbo.Departments WHERE DepartmentCode = 'KCDHA'), (SELECT DepartmentId FROM dbo.Departments WHERE DepartmentCode = 'KCC'), (SELECT UserId FROM dbo.Users WHERE Username = 'truongkhoa'), (SELECT UserId FROM dbo.Users WHERE Username = 'admin'), N'Tăng cường máy siêu âm cho khoa Cấp Cứu mùa dịch bệnh', N'Đã chuyển', '20240216')
) AS sample (AssetId, FromDepartmentId, ToDepartmentId, RequestedBy, ApprovedBy, Reason, Status, TransferDate)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.AssetTransfers AS existing
    WHERE existing.AssetId = sample.AssetId
      AND existing.FromDepartmentId = sample.FromDepartmentId
      AND existing.ToDepartmentId = sample.ToDepartmentId
      AND existing.Reason = sample.Reason
);

INSERT INTO dbo.UsageLogs (AssetId, LogDate, UsageCount, Unit, RecordedBy)
SELECT sample.AssetId, sample.LogDate, sample.UsageCount, sample.Unit, sample.RecordedBy
FROM (VALUES
    ((SELECT AssetId FROM dbo.EquipmentAssets WHERE AssetCode = 'CT-001'), '2024-03-01', 85, N'Ca chụp', (SELECT UserId FROM dbo.Users WHERE Username = 'truongkhoa')),
    ((SELECT AssetId FROM dbo.EquipmentAssets WHERE AssetCode = 'CT-001'), '2024-03-02', 90, N'Ca chụp', (SELECT UserId FROM dbo.Users WHERE Username = 'truongkhoa')),
    ((SELECT AssetId FROM dbo.EquipmentAssets WHERE AssetCode = 'CT-001'), '2024-03-03', 110, N'Ca chụp', (SELECT UserId FROM dbo.Users WHERE Username = 'truongkhoa'))
) AS sample (AssetId, LogDate, UsageCount, Unit, RecordedBy)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.UsageLogs AS existing
    WHERE existing.AssetId = sample.AssetId
      AND existing.LogDate = sample.LogDate
);

-- The sample stock starts at 49 after the historical use of one cable.
INSERT INTO dbo.Maintenance_SpareParts (MaintenanceId, PartId, QuantityUsed, Note)
SELECT m.MaintenanceId, p.PartId, 1, N'Thay cáp SpO2 mới do đứt ngầm'
FROM (
    SELECT TOP (1) m.MaintenanceId
    FROM dbo.MaintenanceRecords AS m
    JOIN dbo.EquipmentAssets AS a ON a.AssetId = m.AssetId
    WHERE a.AssetCode = 'MON-001'
      AND m.MaintenanceType = N'Sửa chữa đột xuất'
      AND m.MaintenanceDate = '20240310'
    ORDER BY m.MaintenanceId
) AS m
CROSS JOIN dbo.SpareParts AS p
WHERE p.PartCode = 'SP-SPO2-01'
  AND NOT EXISTS (
      SELECT 1 FROM dbo.Maintenance_SpareParts AS existing
      WHERE existing.MaintenanceId = m.MaintenanceId AND existing.PartId = p.PartId
  );

COMMIT TRANSACTION;
GO
