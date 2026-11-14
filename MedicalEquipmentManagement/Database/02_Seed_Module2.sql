-- =============================================================================
-- SCRIPT KHỞI TẠO & BỔ SUNG DỮ LIỆU MẪU PHONG PHÚ CHO MODULE 2 (DANH MỤC & THIẾT BỊ)
-- Đảm bảo chạy an toàn nhiều lần (Idempotent - IF NOT EXISTS)
-- Cung cấp > 20 thiết bị vật lý để kiểm thử Tìm kiếm, Lọc, Phân trang (Pagination),
-- QR/Barcode và Kiểm thử chặn Xóa thiết bị đang có Lịch sử bảo trì (FK Constraint).
-- =============================================================================

USE MedicalEquipmentDB;
GO

-- 1. Bổ sung thêm Phòng ban (Departments)
IF NOT EXISTS (SELECT 1 FROM Departments WHERE DepartmentCode = 'KHSCC')
    INSERT INTO Departments (DepartmentCode, Name, Location, IsActive)
    VALUES ('KHSCC', N'Khoa Hồi sức tích cực (ICU)', N'Tầng 2 - Tòa A', 1);

IF NOT EXISTS (SELECT 1 FROM Departments WHERE DepartmentCode = 'KGMHS')
    INSERT INTO Departments (DepartmentCode, Name, Location, IsActive)
    VALUES ('KGMHS', N'Khoa Phẫu thuật - Gây mê hồi sức', N'Tầng 3 - Tòa A', 1);

IF NOT EXISTS (SELECT 1 FROM Departments WHERE DepartmentCode = 'KNOI')
    INSERT INTO Departments (DepartmentCode, Name, Location, IsActive)
    VALUES ('KNOI', N'Khoa Nội Tim Mạch', N'Tầng 4 - Tòa B', 1);
GO

-- 2. Bổ sung thêm Danh mục loại thiết bị (EquipmentCategories)
IF NOT EXISTS (SELECT 1 FROM EquipmentCategories WHERE CategoryCode = 'THO')
    INSERT INTO EquipmentCategories (CategoryCode, Name, IsActive)
    VALUES ('THO', N'Máy thở & Hồi sức hô hấp', 1);

IF NOT EXISTS (SELECT 1 FROM EquipmentCategories WHERE CategoryCode = 'XN')
    INSERT INTO EquipmentCategories (CategoryCode, Name, IsActive)
    VALUES ('XN', N'Thiết bị Xét nghiệm sinh hóa - huyết học', 1);

IF NOT EXISTS (SELECT 1 FROM EquipmentCategories WHERE CategoryCode = 'GM')
    INSERT INTO EquipmentCategories (CategoryCode, Name, IsActive)
    VALUES ('GM', N'Máy gây mê kèm thở & Bơm tiêm điện', 1);

IF NOT EXISTS (SELECT 1 FROM EquipmentCategories WHERE CategoryCode = 'ECG')
    INSERT INTO EquipmentCategories (CategoryCode, Name, IsActive)
    VALUES ('ECG', N'Máy điện tim & Khử rung tim', 1);
GO

-- 3. Bổ sung thêm Nhà cung cấp & Hãng sản xuất (Suppliers)
IF NOT EXISTS (SELECT 1 FROM Suppliers WHERE Name = N'GE HealthCare')
    INSERT INTO Suppliers (Name, SupplierType, Phone, Email, IsActive)
    VALUES (N'GE HealthCare', N'Manufacturer', '18003333', 'vietnam@gehealthcare.com', 1);

IF NOT EXISTS (SELECT 1 FROM Suppliers WHERE Name = N'Dräger Medical (Đức)')
    INSERT INTO Suppliers (Name, SupplierType, Phone, Email, IsActive)
    VALUES (N'Dräger Medical (Đức)', N'Manufacturer', '18004444', 'info@draeger.com', 1);

IF NOT EXISTS (SELECT 1 FROM Suppliers WHERE Name = N'Roche Diagnostics')
    INSERT INTO Suppliers (Name, SupplierType, Phone, Email, IsActive)
    VALUES (N'Roche Diagnostics', N'Manufacturer', '18005555', 'support@roche.com', 1);

IF NOT EXISTS (SELECT 1 FROM Suppliers WHERE Name = N'Nihon Kohden (Nhật Bản)')
    INSERT INTO Suppliers (Name, SupplierType, Phone, Email, IsActive)
    VALUES (N'Nihon Kohden (Nhật Bản)', N'Manufacturer', '18006666', 'info@nihonkohden.com', 1);

IF NOT EXISTS (SELECT 1 FROM Suppliers WHERE Name = N'Công ty CP Thiết bị Y tế Việt Nhật (JVC)')
    INSERT INTO Suppliers (Name, SupplierType, Phone, Email, IsActive)
    VALUES (N'Công ty CP Thiết bị Y tế Việt Nhật (JVC)', N'Vendor', '02437778888', 'contact@ytevietnhat.vn', 1);
GO

-- 4. Bổ sung thêm Mẫu thiết bị (EquipmentModels)
DECLARE @CatXQ INT = (SELECT TOP 1 CategoryId FROM EquipmentCategories WHERE CategoryCode = 'XQ');
DECLARE @CatSA INT = (SELECT TOP 1 CategoryId FROM EquipmentCategories WHERE CategoryCode = 'SA');
DECLARE @CatMON INT = (SELECT TOP 1 CategoryId FROM EquipmentCategories WHERE CategoryCode = 'MON');
DECLARE @CatTHO INT = (SELECT TOP 1 CategoryId FROM EquipmentCategories WHERE CategoryCode = 'THO');
DECLARE @CatXN INT = (SELECT TOP 1 CategoryId FROM EquipmentCategories WHERE CategoryCode = 'XN');
DECLARE @CatGM INT = (SELECT TOP 1 CategoryId FROM EquipmentCategories WHERE CategoryCode = 'GM');
DECLARE @CatECG INT = (SELECT TOP 1 CategoryId FROM EquipmentCategories WHERE CategoryCode = 'ECG');

DECLARE @MfgPhilips INT = (SELECT TOP 1 SupplierId FROM Suppliers WHERE Name LIKE N'%Philips%');
DECLARE @MfgSiemens INT = (SELECT TOP 1 SupplierId FROM Suppliers WHERE Name LIKE N'%Siemens%');
DECLARE @MfgGE INT = (SELECT TOP 1 SupplierId FROM Suppliers WHERE Name LIKE N'%GE HealthCare%');
DECLARE @MfgDraeger INT = (SELECT TOP 1 SupplierId FROM Suppliers WHERE Name LIKE N'%Dräger%');
DECLARE @MfgRoche INT = (SELECT TOP 1 SupplierId FROM Suppliers WHERE Name LIKE N'%Roche%');
DECLARE @MfgNihon INT = (SELECT TOP 1 SupplierId FROM Suppliers WHERE Name LIKE N'%Nihon Kohden%');

IF NOT EXISTS (SELECT 1 FROM EquipmentModels WHERE ModelCode = 'DR-EVITA-V300')
    INSERT INTO EquipmentModels (ModelCode, Name, CategoryId, ManufacturerId, IsActive)
    VALUES ('DR-EVITA-V300', N'Máy thở cao cấp Dräger Evita V300', @CatTHO, @MfgDraeger, 1);

IF NOT EXISTS (SELECT 1 FROM EquipmentModels WHERE ModelCode = 'GE-LOGIQ-E10')
    INSERT INTO EquipmentModels (ModelCode, Name, CategoryId, ManufacturerId, IsActive)
    VALUES ('GE-LOGIQ-E10', N'Máy siêu âm đa chiều GE LOGIQ E10', @CatSA, @MfgGE, 1);

IF NOT EXISTS (SELECT 1 FROM EquipmentModels WHERE ModelCode = 'RC-COBAS-C311')
    INSERT INTO EquipmentModels (ModelCode, Name, CategoryId, ManufacturerId, IsActive)
    VALUES ('RC-COBAS-C311', N'Máy xét nghiệm sinh hóa tự động Roche Cobas c311', @CatXN, @MfgRoche, 1);

IF NOT EXISTS (SELECT 1 FROM EquipmentModels WHERE ModelCode = 'DR-FABIUS-PLUS')
    INSERT INTO EquipmentModels (ModelCode, Name, CategoryId, ManufacturerId, IsActive)
    VALUES ('DR-FABIUS-PLUS', N'Máy gây mê kèm thở Dräger Fabius Plus', @CatGM, @MfgDraeger, 1);

IF NOT EXISTS (SELECT 1 FROM EquipmentModels WHERE ModelCode = 'NK-ECG-2150')
    INSERT INTO EquipmentModels (ModelCode, Name, CategoryId, ManufacturerId, IsActive)
    VALUES ('NK-ECG-2150', N'Máy điện tim 3 cần Nihon Kohden ECG-2150', @CatECG, @MfgNihon, 1);

IF NOT EXISTS (SELECT 1 FROM EquipmentModels WHERE ModelCode = 'NK-TEC-5631')
    INSERT INTO EquipmentModels (ModelCode, Name, CategoryId, ManufacturerId, IsActive)
    VALUES ('NK-TEC-5631', N'Máy sốc điện phá rung tim Nihon Kohden TEC-5631', @CatECG, @MfgNihon, 1);
GO

-- 5. Bổ sung thêm Hợp đồng (Contracts)
DECLARE @VendorJVC INT = (SELECT TOP 1 SupplierId FROM Suppliers WHERE Name LIKE N'%Việt Nhật%');
IF @VendorJVC IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Contracts WHERE ContractNumber = 'HD-2024-002')
    INSERT INTO Contracts (ContractNumber, SupplierId, StartDate, EndDate, ContractValue, ContractType)
    VALUES ('HD-2024-002', @VendorJVC, '2024-01-15', '2027-01-15', 12500000000, N'Mua sắm & Bảo hành 36 tháng');
GO

-- 6. Bổ sung thêm danh sách Tài sản thực tế (EquipmentAssets) để đủ > 22 bản ghi phục vụ Phân trang
DECLARE @DeptCC INT = (SELECT TOP 1 DepartmentId FROM Departments WHERE DepartmentCode = 'KCC');
DECLARE @DeptCDHA INT = (SELECT TOP 1 DepartmentId FROM Departments WHERE DepartmentCode = 'KCDHA');
DECLARE @DeptXN INT = (SELECT TOP 1 DepartmentId FROM Departments WHERE DepartmentCode = 'KXN');
DECLARE @DeptICU INT = (SELECT TOP 1 DepartmentId FROM Departments WHERE DepartmentCode = 'KHSCC');
DECLARE @DeptGM INT = (SELECT TOP 1 DepartmentId FROM Departments WHERE DepartmentCode = 'KGMHS');
DECLARE @DeptNOI INT = (SELECT TOP 1 DepartmentId FROM Departments WHERE DepartmentCode = 'KNOI');

DECLARE @ModEpiq7 INT = (SELECT TOP 1 ModelId FROM EquipmentModels WHERE ModelCode = 'PH-EPIQ7');
DECLARE @ModSomatom INT = (SELECT TOP 1 ModelId FROM EquipmentModels WHERE ModelCode = 'SM-SOMATOM');
DECLARE @ModMX40 INT = (SELECT TOP 1 ModelId FROM EquipmentModels WHERE ModelCode = 'PH-MX40');
DECLARE @ModEvita INT = (SELECT TOP 1 ModelId FROM EquipmentModels WHERE ModelCode = 'DR-EVITA-V300');
DECLARE @ModLogiq INT = (SELECT TOP 1 ModelId FROM EquipmentModels WHERE ModelCode = 'GE-LOGIQ-E10');
DECLARE @ModCobas INT = (SELECT TOP 1 ModelId FROM EquipmentModels WHERE ModelCode = 'RC-COBAS-C311');
DECLARE @ModFabius INT = (SELECT TOP 1 ModelId FROM EquipmentModels WHERE ModelCode = 'DR-FABIUS-PLUS');
DECLARE @ModECG INT = (SELECT TOP 1 ModelId FROM EquipmentModels WHERE ModelCode = 'NK-ECG-2150');
DECLARE @ModDefib INT = (SELECT TOP 1 ModelId FROM EquipmentModels WHERE ModelCode = 'NK-TEC-5631');

DECLARE @Contract1 INT = (SELECT TOP 1 ContractId FROM Contracts WHERE ContractNumber = 'HD-2023-001');
DECLARE @Contract2 INT = (SELECT TOP 1 ContractId FROM Contracts WHERE ContractNumber = 'HD-2024-002');

IF NOT EXISTS (SELECT 1 FROM EquipmentAssets WHERE AssetCode = 'SA-003')
    INSERT INTO EquipmentAssets (AssetCode, SerialNumber, ModelId, DepartmentId, ContractId, PurchaseDate, PurchasePrice, Status, IsDeleted)
    VALUES ('SA-003', 'SN-GE-LQ1001', @ModLogiq, @DeptCDHA, @Contract2, '2024-02-10', 1850000000, N'Đang hoạt động', 0);

IF NOT EXISTS (SELECT 1 FROM EquipmentAssets WHERE AssetCode = 'SA-004')
    INSERT INTO EquipmentAssets (AssetCode, SerialNumber, ModelId, DepartmentId, ContractId, PurchaseDate, PurchasePrice, Status, IsDeleted)
    VALUES ('SA-004', 'SN-GE-LQ1002', @ModLogiq, @DeptNOI, @Contract2, '2024-02-10', 1850000000, N'Đang hoạt động', 0);

IF NOT EXISTS (SELECT 1 FROM EquipmentAssets WHERE AssetCode = 'THO-001')
    INSERT INTO EquipmentAssets (AssetCode, SerialNumber, ModelId, DepartmentId, ContractId, PurchaseDate, PurchasePrice, Status, IsDeleted)
    VALUES ('THO-001', 'SN-DR-EV3001', @ModEvita, @DeptICU, @Contract2, '2024-01-20', 980000000, N'Đang hoạt động', 0);

IF NOT EXISTS (SELECT 1 FROM EquipmentAssets WHERE AssetCode = 'THO-002')
    INSERT INTO EquipmentAssets (AssetCode, SerialNumber, ModelId, DepartmentId, ContractId, PurchaseDate, PurchasePrice, Status, IsDeleted)
    VALUES ('THO-002', 'SN-DR-EV3002', @ModEvita, @DeptICU, @Contract2, '2024-01-20', 980000000, N'Đang hoạt động', 0);

IF NOT EXISTS (SELECT 1 FROM EquipmentAssets WHERE AssetCode = 'THO-003')
    INSERT INTO EquipmentAssets (AssetCode, SerialNumber, ModelId, DepartmentId, ContractId, PurchaseDate, PurchasePrice, Status, IsDeleted)
    VALUES ('THO-003', 'SN-DR-EV3003', @ModEvita, @DeptCC, @Contract2, '2024-01-20', 980000000, N'Đang bảo trì', 0);

IF NOT EXISTS (SELECT 1 FROM EquipmentAssets WHERE AssetCode = 'XN-001')
    INSERT INTO EquipmentAssets (AssetCode, SerialNumber, ModelId, DepartmentId, ContractId, PurchaseDate, PurchasePrice, Status, IsDeleted)
    VALUES ('XN-001', 'SN-RC-CB31101', @ModCobas, @DeptXN, @Contract1, '2023-05-12', 2400000000, N'Đang hoạt động', 0);

IF NOT EXISTS (SELECT 1 FROM EquipmentAssets WHERE AssetCode = 'XN-002')
    INSERT INTO EquipmentAssets (AssetCode, SerialNumber, ModelId, DepartmentId, ContractId, PurchaseDate, PurchasePrice, Status, IsDeleted)
    VALUES ('XN-002', 'SN-RC-CB31102', @ModCobas, @DeptXN, @Contract1, '2023-05-12', 2400000000, N'Đang hoạt động', 0);

IF NOT EXISTS (SELECT 1 FROM EquipmentAssets WHERE AssetCode = 'GM-001')
    INSERT INTO EquipmentAssets (AssetCode, SerialNumber, ModelId, DepartmentId, ContractId, PurchaseDate, PurchasePrice, Status, IsDeleted)
    VALUES ('GM-001', 'SN-DR-FB8801', @ModFabius, @DeptGM, @Contract2, '2024-03-01', 1650000000, N'Đang hoạt động', 0);

IF NOT EXISTS (SELECT 1 FROM EquipmentAssets WHERE AssetCode = 'GM-002')
    INSERT INTO EquipmentAssets (AssetCode, SerialNumber, ModelId, DepartmentId, ContractId, PurchaseDate, PurchasePrice, Status, IsDeleted)
    VALUES ('GM-002', 'SN-DR-FB8802', @ModFabius, @DeptGM, @Contract2, '2024-03-01', 1650000000, N'Đang hoạt động', 0);

IF NOT EXISTS (SELECT 1 FROM EquipmentAssets WHERE AssetCode = 'MON-002')
    INSERT INTO EquipmentAssets (AssetCode, SerialNumber, ModelId, DepartmentId, ContractId, PurchaseDate, PurchasePrice, Status, IsDeleted)
    VALUES ('MON-002', 'SN-MX40-112', @ModMX40, @DeptICU, @Contract1, '2023-08-15', 120000000, N'Đang hoạt động', 0);

IF NOT EXISTS (SELECT 1 FROM EquipmentAssets WHERE AssetCode = 'MON-003')
    INSERT INTO EquipmentAssets (AssetCode, SerialNumber, ModelId, DepartmentId, ContractId, PurchaseDate, PurchasePrice, Status, IsDeleted)
    VALUES ('MON-003', 'SN-MX40-113', @ModMX40, @DeptICU, @Contract1, '2023-08-15', 120000000, N'Đang hoạt động', 0);

IF NOT EXISTS (SELECT 1 FROM EquipmentAssets WHERE AssetCode = 'MON-004')
    INSERT INTO EquipmentAssets (AssetCode, SerialNumber, ModelId, DepartmentId, ContractId, PurchaseDate, PurchasePrice, Status, IsDeleted)
    VALUES ('MON-004', 'SN-MX40-114', @ModMX40, @DeptNOI, @Contract1, '2023-08-15', 120000000, N'Hỏng', 0);

IF NOT EXISTS (SELECT 1 FROM EquipmentAssets WHERE AssetCode = 'ECG-001')
    INSERT INTO EquipmentAssets (AssetCode, SerialNumber, ModelId, DepartmentId, ContractId, PurchaseDate, PurchasePrice, Status, IsDeleted)
    VALUES ('ECG-001', 'SN-NK-ECG001', @ModECG, @DeptNOI, @Contract2, '2024-02-01', 65000000, N'Đang hoạt động', 0);

IF NOT EXISTS (SELECT 1 FROM EquipmentAssets WHERE AssetCode = 'ECG-002')
    INSERT INTO EquipmentAssets (AssetCode, SerialNumber, ModelId, DepartmentId, ContractId, PurchaseDate, PurchasePrice, Status, IsDeleted)
    VALUES ('ECG-002', 'SN-NK-ECG002', @ModECG, @DeptCC, @Contract2, '2024-02-01', 65000000, N'Đang hoạt động', 0);

IF NOT EXISTS (SELECT 1 FROM EquipmentAssets WHERE AssetCode = 'ECG-003')
    INSERT INTO EquipmentAssets (AssetCode, SerialNumber, ModelId, DepartmentId, ContractId, PurchaseDate, PurchasePrice, Status, IsDeleted)
    VALUES ('ECG-003', 'SN-NK-ECG003', @ModECG, NULL, NULL, '2021-06-10', 58000000, N'Đã thanh lý', 0);

IF NOT EXISTS (SELECT 1 FROM EquipmentAssets WHERE AssetCode = 'DFB-001')
    INSERT INTO EquipmentAssets (AssetCode, SerialNumber, ModelId, DepartmentId, ContractId, PurchaseDate, PurchasePrice, Status, IsDeleted)
    VALUES ('DFB-001', 'SN-NK-DFB501', @ModDefib, @DeptCC, @Contract2, '2024-02-18', 210000000, N'Đang hoạt động', 0);

IF NOT EXISTS (SELECT 1 FROM EquipmentAssets WHERE AssetCode = 'DFB-002')
    INSERT INTO EquipmentAssets (AssetCode, SerialNumber, ModelId, DepartmentId, ContractId, PurchaseDate, PurchasePrice, Status, IsDeleted)
    VALUES ('DFB-002', 'SN-NK-DFB502', @ModDefib, @DeptICU, @Contract2, '2024-02-18', 210000000, N'Đang hoạt động', 0);

IF NOT EXISTS (SELECT 1 FROM EquipmentAssets WHERE AssetCode = 'CT-002')
    INSERT INTO EquipmentAssets (AssetCode, SerialNumber, ModelId, DepartmentId, ContractId, PurchaseDate, PurchasePrice, Status, IsDeleted)
    VALUES ('CT-002', 'SN-SM-CT882', @ModSomatom, @DeptCDHA, @Contract1, '2023-11-20', 8600000000, N'Đang hoạt động', 0);
GO

PRINT N'>>> ĐÃ CẬP NHẬT DỮ LIỆU MẪU MODULE 2 THÀNH CÔNG! <<<';
GO
