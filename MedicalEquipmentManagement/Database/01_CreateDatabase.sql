-- SQL Server 2016+
IF DB_ID(N'MedicalEquipmentDB') IS NULL
    CREATE DATABASE MedicalEquipmentDB;
GO
USE MedicalEquipmentDB;
GO

IF OBJECT_ID('dbo.Suppliers','U') IS NULL
CREATE TABLE Suppliers(
    SupplierId INT IDENTITY PRIMARY KEY,
    Name NVARCHAR(150) NOT NULL,
    Phone NVARCHAR(30) NULL,
    Email NVARCHAR(150) NULL,
    Address NVARCHAR(250) NULL
);

IF OBJECT_ID('dbo.EquipmentCategories','U') IS NULL
CREATE TABLE EquipmentCategories(
    CategoryId INT IDENTITY PRIMARY KEY,
    Name NVARCHAR(150) NOT NULL,
    Description NVARCHAR(500) NULL
);

IF OBJECT_ID('dbo.Departments','U') IS NULL
CREATE TABLE Departments(
    DepartmentId INT IDENTITY PRIMARY KEY,
    Name NVARCHAR(150) NOT NULL,
    Location NVARCHAR(250) NULL
);

IF OBJECT_ID('dbo.Equipments','U') IS NULL
CREATE TABLE Equipments(
    EquipmentId INT IDENTITY PRIMARY KEY,
    EquipmentCode NVARCHAR(50) NOT NULL UNIQUE,
    Name NVARCHAR(200) NOT NULL,
    CategoryId INT NOT NULL,
    SupplierId INT NULL,
    DepartmentId INT NULL,
    PurchaseDate DATE NULL,
    PurchasePrice DECIMAL(18,2) NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT N'Đang sử dụng',
    Quantity INT NOT NULL DEFAULT 1,
    ImagePath NVARCHAR(250) NULL,
    Description NVARCHAR(1000) NULL,
    CONSTRAINT FK_Equipments_Categories FOREIGN KEY(CategoryId) REFERENCES EquipmentCategories(CategoryId),
    CONSTRAINT FK_Equipments_Suppliers FOREIGN KEY(SupplierId) REFERENCES Suppliers(SupplierId),
    CONSTRAINT FK_Equipments_Departments FOREIGN KEY(DepartmentId) REFERENCES Departments(DepartmentId)
);

IF OBJECT_ID('dbo.MaintenanceRecords','U') IS NULL
CREATE TABLE MaintenanceRecords(
    MaintenanceId INT IDENTITY PRIMARY KEY,
    EquipmentId INT NOT NULL,
    MaintenanceDate DATE NOT NULL,
    NextMaintenanceDate DATE NULL,
    Description NVARCHAR(1000) NULL,
    Cost DECIMAL(18,2) NULL,
    Result NVARCHAR(100) NULL,
    CONSTRAINT FK_Maintenance_Equipment FOREIGN KEY(EquipmentId) REFERENCES Equipments(EquipmentId)
);

IF OBJECT_ID('dbo.InventoryTransactions','U') IS NULL
CREATE TABLE InventoryTransactions(
    TransactionId INT IDENTITY PRIMARY KEY,
    EquipmentId INT NOT NULL,
    DepartmentId INT NULL,
    TransactionType NVARCHAR(30) NOT NULL,
    Quantity INT NOT NULL,
    TransactionDate DATETIME NOT NULL DEFAULT GETDATE(),
    Note NVARCHAR(500) NULL,
    CONSTRAINT FK_Inventory_Equipment FOREIGN KEY(EquipmentId) REFERENCES Equipments(EquipmentId),
    CONSTRAINT FK_Inventory_Department FOREIGN KEY(DepartmentId) REFERENCES Departments(DepartmentId)
);

IF OBJECT_ID('dbo.Users','U') IS NULL
CREATE TABLE Users(
    UserId INT IDENTITY PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    FullName NVARCHAR(150) NOT NULL,
    Role NVARCHAR(30) NOT NULL DEFAULT N'Staff',
    IsActive BIT NOT NULL DEFAULT 1
);
GO
