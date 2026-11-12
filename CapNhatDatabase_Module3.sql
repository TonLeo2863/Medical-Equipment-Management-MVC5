USE MedicalEquipmentDB;
GO

-- 1. Bổ sung các cột cho bảng MaintenanceRecords (Bảo trì) nếu chưa có
IF COL_LENGTH('dbo.MaintenanceRecords', 'NextMaintenanceDate') IS NULL
BEGIN
    ALTER TABLE MaintenanceRecords ADD NextMaintenanceDate DATE NULL;
    PRINT N'Đã thêm cột NextMaintenanceDate vào MaintenanceRecords';
END
GO

IF COL_LENGTH('dbo.MaintenanceRecords', 'Description') IS NULL
BEGIN
    ALTER TABLE MaintenanceRecords ADD Description NVARCHAR(500) NULL;
    PRINT N'Đã thêm cột Description vào MaintenanceRecords';
END
GO

IF COL_LENGTH('dbo.MaintenanceRecords', 'TicketId') IS NULL
BEGIN
    ALTER TABLE MaintenanceRecords ADD TicketId INT NULL;
    PRINT N'Đã thêm cột TicketId vào MaintenanceRecords';
END
GO

-- 2. Đảm bảo bảng InventoryTransactions tồn tại (Nhật ký kho)
IF OBJECT_ID('dbo.InventoryTransactions','U') IS NULL
BEGIN
    CREATE TABLE InventoryTransactions(
        TransactionId INT IDENTITY PRIMARY KEY,
        EquipmentId INT NOT NULL,
        DepartmentId INT NULL,
        TransactionType NVARCHAR(50) NOT NULL,
        Quantity INT NOT NULL DEFAULT 1,
        TransactionDate DATETIME NOT NULL DEFAULT GETDATE(),
        Note NVARCHAR(500) NULL
    );
    PRINT N'Đã tạo bảng InventoryTransactions';
END
GO

PRINT N'>>> CẬP NHẬT DATABASE CHO MODULE 3 HOÀN TẤT THÀNH CÔNG! <<<';
GO
