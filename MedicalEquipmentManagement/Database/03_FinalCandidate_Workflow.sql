SET NOCOUNT ON;
SET XACT_ABORT ON;
-- Chặn nhầm database trước mọi lệnh DDL. XACT_ABORT hủy transaction khi SQL phát sinh lỗi.
IF DB_NAME() <> N'MedicalEquipmentDB_BetaTest'
    THROW 51000, 'Expected MedicalEquipmentDB_BetaTest.', 1;

BEGIN TRANSACTION;

IF COL_LENGTH('dbo.WorkOrders', 'AssignedToUserId') IS NULL
    ALTER TABLE dbo.WorkOrders ADD AssignedToUserId int NULL
        CONSTRAINT FK_WorkOrders_AssignedUser REFERENCES dbo.Users(UserId);

IF COL_LENGTH('dbo.WorkOrders', 'ReporterVerified') IS NULL
    -- Giữ nguyên người gắn trên phiếu cũ vì ứng dụng Beta có thể đã lưu kỹ sư vào cột ReportedBy.
    ALTER TABLE dbo.WorkOrders ADD ReporterVerified bit NOT NULL
        CONSTRAINT DF_WorkOrders_ReporterVerified DEFAULT (0);

IF COL_LENGTH('dbo.WorkOrders', 'Resolution') IS NULL
    ALTER TABLE dbo.WorkOrders ADD Resolution nvarchar(500) NULL, TechnicalPassed bit NULL;

IF COL_LENGTH('dbo.WorkOrders', 'ResolvedByUserId') IS NULL
    ALTER TABLE dbo.WorkOrders ADD ResolvedByUserId int NULL
        CONSTRAINT FK_WorkOrders_ResolvedUser REFERENCES dbo.Users(UserId);

IF COL_LENGTH('dbo.MaintenanceRecords', 'PerformedByUserId') IS NULL
    ALTER TABLE dbo.MaintenanceRecords ADD PerformedByUserId int NULL
        CONSTRAINT FK_MaintenanceRecords_PerformedBy REFERENCES dbo.Users(UserId);

IF OBJECT_ID('dbo.FK_MaintenanceRecords_WorkOrders', 'F') IS NULL
    ALTER TABLE dbo.MaintenanceRecords WITH CHECK ADD CONSTRAINT FK_MaintenanceRecords_WorkOrders
        FOREIGN KEY (TicketId) REFERENCES dbo.WorkOrders(TicketId);

UPDATE dbo.WorkOrders SET Status = N'Đã xong' WHERE Status = N'Đã hoàn thành';

COMMIT TRANSACTION;
