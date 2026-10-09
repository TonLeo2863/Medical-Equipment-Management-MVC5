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

-- Cập nhật bảng MaintenanceRecords để liên kết với WorkOrders (Nếu sửa chữa xuất phát từ báo hỏng)
IF COL_LENGTH('dbo.MaintenanceRecords', 'TicketId') IS NULL
BEGIN
    ALTER TABLE MaintenanceRecords ADD TicketId INT NULL;
    ALTER TABLE MaintenanceRecords ADD CONSTRAINT FK_Maintenance_WorkOrders FOREIGN KEY(TicketId) REFERENCES WorkOrders(TicketId);
END

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

-- Chèn Role
INSERT INTO Roles (RoleCode, RoleName, Description) VALUES 
('ADMIN', N'Quản trị hệ thống', N'Toàn quyền'),
('MANAGER', N'Trưởng khoa/Phòng vật tư', N'Quản lý thiết bị và duyệt luân chuyển'),
('ENGINEER', N'Kỹ sư y sinh', N'Cập nhật bảo trì, sửa chữa'),
('STAFF', N'Nhân viên y tế', N'Xem thiết bị tại khoa');

-- Chèn User (PasswordHash minh họa là '123' đã mã hóa giả lập)
INSERT INTO Users (Username, PasswordHash, FullName, RoleId, Email) VALUES 
('admin', 'hash_123', N'Quản trị viên', 1, 'admin@hospital.vn'),
('truongkhoa', 'hash_123', N'BS. Nguyễn Văn A', 2, 'khoa_a@hospital.vn'),
('kysu1', 'hash_123', N'KS. Trần Văn B', 3, 'kysu@hospital.vn');

-- Chèn Phòng ban
INSERT INTO Departments (DepartmentCode, Name, Location) VALUES 
('KCC', N'Khoa Cấp Cứu', N'Tầng 1 - Tòa A'),
('KCDHA', N'Khoa Chẩn đoán hình ảnh', N'Tầng 1 - Tòa B'),
('KXN', N'Khoa Xét nghiệm', N'Tầng 2 - Tòa B');

-- Chèn Nhà cung cấp & Hãng SX
INSERT INTO Suppliers (Name, SupplierType, Phone, Email) VALUES 
('Philips Healthcare', 'Manufacturer', '18001111', 'contact@philips.com'),
('Siemens Healthineers', 'Manufacturer', '18002222', 'contact@siemens.com'),
('Công ty TNHH TBYT Phương Đông', 'Vendor', '02833334444', 'sales@phuongdong.vn');

-- Chèn Danh mục
INSERT INTO EquipmentCategories (CategoryCode, Name) VALUES 
('XQ', N'Hệ thống X-Quang'),
('SA', N'Máy Siêu Âm'),
('MON', N'Monitor theo dõi bệnh nhân');

-- Chèn Model
INSERT INTO EquipmentModels (ModelCode, Name, CategoryId, ManufacturerId) VALUES 
('PH-EPIQ7', 'Philips EPIQ 7 Ultrasound', 2, 1),
('SM-SOMATOM', 'Siemens SOMATOM CT Scanner', 1, 2),
('PH-MX40', 'Philips IntelliVue MX40', 3, 1);

-- Chèn Hợp đồng
INSERT INTO Contracts (ContractNumber, SupplierId, StartDate, EndDate, ContractType) VALUES 
('HD-2023-001', 3, '2023-01-01', '2026-12-31', N'Bảo trì toàn diện');

-- Chèn Thiết bị vật lý (Assets)
INSERT INTO EquipmentAssets (AssetCode, SerialNumber, ModelId, DepartmentId, ContractId, PurchaseDate, PurchasePrice, Status) VALUES 
('SA-001', 'SN-PH7-9901', 1, 2, 1, '2023-02-15', 1500000000, N'Đang hoạt động'),
('SA-002', 'SN-PH7-9902', 1, 2, 1, '2023-02-15', 1500000000, N'Đang hoạt động'),
('CT-001', 'SN-SM-CT881', 2, 2, 1, '2022-10-10', 8500000000, N'Đang hoạt động'),
('MON-001', 'SN-MX40-111', 3, 1, NULL, '2024-01-05', 120000000, N'Đang bảo trì');

-- Chèn Lịch sử Bảo trì
INSERT INTO MaintenanceRecords (AssetId, MaintenanceType, MaintenanceDate, Cost, ResultStatus) VALUES 
(4, N'Sửa chữa đột xuất', '2024-03-10', 5500000, N'Đang chờ linh kiện'),
(1, N'Bảo dưỡng định kỳ', '2024-02-15', 0, N'Đạt yêu cầu (Thuộc HĐ)');

-- Chèn Nhật ký Kiểm định
INSERT INTO CalibrationLogs (AssetId, CalibrationDate, NextCalibrationDate, PerformedBy, CertificateNumber, Result) VALUES 
(1, '2023-02-20', '2024-02-20', N'Viện Trang thiết bị và Công trình y tế', 'KD-23-0012', N'Đạt'),
(3, '2022-10-15', '2023-10-15', N'Trung tâm Kiểm định An toàn Bức xạ', 'BX-22-0991', N'Đạt');
GO
INSERT INTO Users (Username, PasswordHash, FullName, RoleId, Email) 
VALUES ('bs_cuong', 'hash_123', N'BS. Lê Văn Cường', 4, 'bs_cuong@hospital.vn');

-- Chèn Linh kiện (Spare Parts)
INSERT INTO SpareParts (PartCode, PartName, ModelId, StockQuantity, Unit, UnitPrice) VALUES 
('SP-SPO2-01', N'Cáp đo SpO2 dùng nhiều lần', 3, 50, N'Sợi', 1500000),
('SP-CT-TUBE', N'Bóng phát tia X cho CT Somatom', 2, 1, N'Cái', 2500000000),
('SP-ULTRA-PR', N'Đầu dò siêu âm Convex', 1, 3, N'Cái', 120000000);

-- Chèn Phiếu sự cố (Work Orders)
-- VD: Bác sĩ Cường ở khoa Cấp Cứu báo hỏng Monitor
INSERT INTO WorkOrders (TicketCode, AssetId, ReportedBy, ReportedDate, IssueDescription, UrgencyLevel, Status) VALUES 
('WO-2024-001', 4, 4, '2024-03-09 08:30:00', N'Máy Monitor mất tín hiệu đo SpO2, màn hình thỉnh thoảng chớp nháy.', N'Cao', N'Đã hoàn thành');

-- Cập nhật lại MaintenanceRecord cũ (AssetId = 4) để liên kết với WorkOrder vừa tạo
UPDATE MaintenanceRecords 
SET TicketId = (SELECT TicketId FROM WorkOrders WHERE TicketCode = 'WO-2024-001')
WHERE AssetId = 4 AND MaintenanceType = N'Sửa chữa đột xuất';

-- Chèn Chi tiết sử dụng linh kiện cho lần sửa chữa Monitor trên
-- Kỹ sư thay 1 Cáp SpO2
INSERT INTO Maintenance_SpareParts (MaintenanceId, PartId, QuantityUsed, Note) VALUES 
(
    (SELECT MaintenanceId FROM MaintenanceRecords WHERE TicketId = (SELECT TicketId FROM WorkOrders WHERE TicketCode = 'WO-2024-001')), 
    (SELECT PartId FROM SpareParts WHERE PartCode = 'SP-SPO2-01'), 
    1, 
    N'Thay cáp SpO2 mới do đứt ngầm'
);

-- Trừ đi số lượng trong kho
UPDATE SpareParts SET StockQuantity = StockQuantity - 1 WHERE PartCode = 'SP-SPO2-01';

-- Chèn Lịch sử luân chuyển (Asset Transfers)
-- Chuyển một máy Siêu âm (AssetId = 2) từ Khoa CĐHA sang Khoa Cấp Cứu
INSERT INTO AssetTransfers (AssetId, FromDepartmentId, ToDepartmentId, RequestedBy, ApprovedBy, Reason, Status) VALUES 
(2, 2, 1, 2, 1, N'Tăng cường máy siêu âm cho khoa Cấp Cứu mùa dịch bệnh', N'Đã chuyển');

-- Cập nhật lại vị trí hiện tại của thiết bị sau khi chuyển
UPDATE EquipmentAssets SET DepartmentId = 1 WHERE AssetId = 2;

-- Chèn Nhật ký công suất sử dụng (Usage Logs)
-- Ghi nhận số ca chụp CT trong 1 ngày để theo dõi khấu hao bóng X-Quang
INSERT INTO UsageLogs (AssetId, LogDate, UsageCount, Unit, RecordedBy) VALUES 
(3, '2024-03-01', 85, N'Ca chụp', 2),
(3, '2024-03-02', 90, N'Ca chụp', 2),
(3, '2024-03-03', 110, N'Ca chụp', 2);
GO