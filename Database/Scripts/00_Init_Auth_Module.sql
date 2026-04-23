/* ============================================================================
   Medical Appointments System
   Script:       00_Init_Auth_Module.sql
   Database:     MedicalAppointments (SQL Server 2022)
   Module:       Authentication (auth schema)
   Author:       Grupo Desarrollo web Avanzado
   Description:  Tablas basicas para el modulo de auth 

   How to run:
     1. Open SSMS o dbeaver, yo te recomiendo dbeaver xd 
     2. Connect to your SQL Server 2022 instance
     3. Open this file and press F5 (Execute)

   .NET usage:
     Place this file under: /Database/Scripts/00_Init_Auth_Module.sql
     Connection string example (appsettings.json):
       "DefaultConnection": "Server=localhost;Database=MedicalAppointments;
                             Trusted_Connection=True;TrustServerCertificate=True;"
   ============================================================================ */

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

-- ============================================================================
-- 0. Database creation
-- ============================================================================
IF DB_ID(N'MedicalAppointments') IS NULL
BEGIN
    CREATE DATABASE MedicalAppointments;
    PRINT 'Database [MedicalAppointments] created.';
END
ELSE
BEGIN
    PRINT 'Database [MedicalAppointments] already exists. Skipping creation.';
END
GO

USE MedicalAppointments;
GO

-- ============================================================================
-- 1. Schema creation
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'auth')
BEGIN
EXEC('CREATE SCHEMA auth AUTHORIZATION dbo;');
    PRINT 'Schema [auth] created.';
END
ELSE
BEGIN
    PRINT 'Schema [auth] already exists. Skipping creation.';
END
GO

-- ============================================================================
-- 2. Catalog: Roles
-- ============================================================================
IF OBJECT_ID(N'auth.Roles', N'U') IS NULL
BEGIN
CREATE TABLE auth.Roles
(
    RoleId          INT             IDENTITY(1,1) NOT NULL,
    Name            VARCHAR(50)     NOT NULL,
    Description     VARCHAR(250)    NULL,

    CreatedAt       DATETIME2(0)    NOT NULL CONSTRAINT DF_Roles_CreatedAt DEFAULT SYSUTCDATETIME(),
    CreatedBy       INT             NULL,
    UpdatedAt       DATETIME2(0)    NULL,
    UpdatedBy       INT             NULL,
    IsActive        BIT             NOT NULL CONSTRAINT DF_Roles_IsActive DEFAULT 1,

    CONSTRAINT PK_Roles PRIMARY KEY CLUSTERED (RoleId),
    CONSTRAINT UQ_Roles_Name UNIQUE (Name)
);
PRINT 'Table [auth.Roles] created.';
END
GO

-- ============================================================================
-- 3. Catalog: Document Types
-- ============================================================================
IF OBJECT_ID(N'auth.DocumentTypes', N'U') IS NULL
BEGIN
CREATE TABLE auth.DocumentTypes
(
    DocumentTypeId  INT             IDENTITY(1,1) NOT NULL,
    Code            VARCHAR(10)     NOT NULL,
    Name            VARCHAR(80)     NOT NULL,

    CreatedAt       DATETIME2(0)    NOT NULL CONSTRAINT DF_DocumentTypes_CreatedAt DEFAULT SYSUTCDATETIME(),
    CreatedBy       INT             NULL,
    UpdatedAt       DATETIME2(0)    NULL,
    UpdatedBy       INT             NULL,
    IsActive        BIT             NOT NULL CONSTRAINT DF_DocumentTypes_IsActive DEFAULT 1,

    CONSTRAINT PK_DocumentTypes PRIMARY KEY CLUSTERED (DocumentTypeId),
    CONSTRAINT UQ_DocumentTypes_Code UNIQUE (Code)
);
PRINT 'Table [auth.DocumentTypes] created.';
END
GO

-- ============================================================================
-- 4. Persons
-- ============================================================================
IF OBJECT_ID(N'auth.Persons', N'U') IS NULL
BEGIN
CREATE TABLE auth.Persons
(
    PersonId        INT             IDENTITY(1,1) NOT NULL,
    DocumentTypeId  INT             NOT NULL,
    DocumentNumber  VARCHAR(20)     NOT NULL,
    FirstName       VARCHAR(80)     NOT NULL,
    LastName        VARCHAR(80)     NOT NULL,
    BirthDate       DATE            NULL,
    Gender          CHAR(1)         NULL,
    Email           VARCHAR(150)    NOT NULL,
    PhoneNumber     VARCHAR(25)     NULL,

    CreatedAt       DATETIME2(0)    NOT NULL CONSTRAINT DF_Persons_CreatedAt DEFAULT SYSUTCDATETIME(),
    CreatedBy       INT             NULL,
    UpdatedAt       DATETIME2(0)    NULL,
    UpdatedBy       INT             NULL,
    IsActive        BIT             NOT NULL CONSTRAINT DF_Persons_IsActive DEFAULT 1,

    CONSTRAINT PK_Persons PRIMARY KEY CLUSTERED (PersonId),
    CONSTRAINT FK_Persons_DocumentTypes
        FOREIGN KEY (DocumentTypeId) REFERENCES auth.DocumentTypes(DocumentTypeId),
    CONSTRAINT UQ_Persons_Document UNIQUE (DocumentTypeId, DocumentNumber),
    CONSTRAINT UQ_Persons_Email UNIQUE (Email),
    CONSTRAINT CK_Persons_Gender CHECK (Gender IN ('M','F','O') OR Gender IS NULL)
);
PRINT 'Table [auth.Persons] created.';
END
GO

-- ============================================================================
-- 5. Users
-- ============================================================================
IF OBJECT_ID(N'auth.Users', N'U') IS NULL
BEGIN
CREATE TABLE auth.Users
(
    UserId                  INT             IDENTITY(1,1) NOT NULL,
    PersonId                INT             NOT NULL,
    Username                VARCHAR(50)     NOT NULL,
    PasswordHash            VARBINARY(256)  NOT NULL,
    PasswordSalt            VARBINARY(128)  NOT NULL,
    PasswordChangedAt       DATETIME2(0)    NULL,
    MustChangePassword      BIT             NOT NULL CONSTRAINT DF_Users_MustChangePassword DEFAULT 1,

    FailedLoginAttempts     INT             NOT NULL CONSTRAINT DF_Users_FailedLoginAttempts DEFAULT 0,
    LockoutEndUtc           DATETIME2(0)    NULL,
    LastLoginAt             DATETIME2(0)    NULL,
    LastLoginIp             VARCHAR(45)     NULL,

    CreatedAt               DATETIME2(0)    NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT SYSUTCDATETIME(),
    CreatedBy               INT             NULL,
    UpdatedAt               DATETIME2(0)    NULL,
    UpdatedBy               INT             NULL,
    IsActive                BIT             NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT 1,

    CONSTRAINT PK_Users PRIMARY KEY CLUSTERED (UserId),
    CONSTRAINT FK_Users_Persons
        FOREIGN KEY (PersonId) REFERENCES auth.Persons(PersonId),
    CONSTRAINT UQ_Users_Username UNIQUE (Username),
    CONSTRAINT UQ_Users_PersonId UNIQUE (PersonId)
);
PRINT 'Table [auth.Users] created.';
END
GO

-- ============================================================================
-- 6. Profiles
-- ============================================================================
IF OBJECT_ID(N'auth.Profiles', N'U') IS NULL
BEGIN
CREATE TABLE auth.Profiles
(
    ProfileId       INT             IDENTITY(1,1) NOT NULL,
    UserId          INT             NOT NULL,
    EmployeeCode    VARCHAR(30)     NOT NULL,
    JobTitle        VARCHAR(100)    NULL,
    Department      VARCHAR(100)    NULL,
    HireDate        DATE            NULL,
    BaseSalary      DECIMAL(12,2)   NULL,

    CreatedAt       DATETIME2(0)    NOT NULL CONSTRAINT DF_Profiles_CreatedAt DEFAULT SYSUTCDATETIME(),
    CreatedBy       INT             NULL,
    UpdatedAt       DATETIME2(0)    NULL,
    UpdatedBy       INT             NULL,
    IsActive        BIT             NOT NULL CONSTRAINT DF_Profiles_IsActive DEFAULT 1,

    CONSTRAINT PK_Profiles PRIMARY KEY CLUSTERED (ProfileId),
    CONSTRAINT FK_Profiles_Users
        FOREIGN KEY (UserId) REFERENCES auth.Users(UserId),
    CONSTRAINT UQ_Profiles_UserId UNIQUE (UserId),
    CONSTRAINT UQ_Profiles_EmployeeCode UNIQUE (EmployeeCode),
    CONSTRAINT CK_Profiles_BaseSalary CHECK (BaseSalary IS NULL OR BaseSalary >= 0)
);
PRINT 'Table [auth.Profiles] created.';
END
GO

-- ============================================================================
-- 7. UserRoles (many-to-many)
-- ============================================================================
IF OBJECT_ID(N'auth.UserRoles', N'U') IS NULL
BEGIN
CREATE TABLE auth.UserRoles
(
    UserId          INT             NOT NULL,
    RoleId          INT             NOT NULL,

    CreatedAt       DATETIME2(0)    NOT NULL CONSTRAINT DF_UserRoles_CreatedAt DEFAULT SYSUTCDATETIME(),
    CreatedBy       INT             NULL,
    UpdatedAt       DATETIME2(0)    NULL,
    UpdatedBy       INT             NULL,
    IsActive        BIT             NOT NULL CONSTRAINT DF_UserRoles_IsActive DEFAULT 1,

    CONSTRAINT PK_UserRoles PRIMARY KEY CLUSTERED (UserId, RoleId),
    CONSTRAINT FK_UserRoles_Users FOREIGN KEY (UserId) REFERENCES auth.Users(UserId),
    CONSTRAINT FK_UserRoles_Roles FOREIGN KEY (RoleId) REFERENCES auth.Roles(RoleId)
);
PRINT 'Table [auth.UserRoles] created.';
END
GO

-- ============================================================================
-- 8. Audit Foreign Keys (CreatedBy / UpdatedBy -> auth.Users)
--    Added after Users exists. NO ACTION to avoid multiple cascade paths.
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Roles_CreatedBy')
ALTER TABLE auth.Roles ADD CONSTRAINT FK_Roles_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES auth.Users(UserId);
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Roles_UpdatedBy')
ALTER TABLE auth.Roles ADD CONSTRAINT FK_Roles_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES auth.Users(UserId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_DocumentTypes_CreatedBy')
ALTER TABLE auth.DocumentTypes ADD CONSTRAINT FK_DocumentTypes_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES auth.Users(UserId);
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_DocumentTypes_UpdatedBy')
ALTER TABLE auth.DocumentTypes ADD CONSTRAINT FK_DocumentTypes_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES auth.Users(UserId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Persons_CreatedBy')
ALTER TABLE auth.Persons ADD CONSTRAINT FK_Persons_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES auth.Users(UserId);
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Persons_UpdatedBy')
ALTER TABLE auth.Persons ADD CONSTRAINT FK_Persons_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES auth.Users(UserId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Users_CreatedBy')
ALTER TABLE auth.Users ADD CONSTRAINT FK_Users_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES auth.Users(UserId);
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Users_UpdatedBy')
ALTER TABLE auth.Users ADD CONSTRAINT FK_Users_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES auth.Users(UserId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Profiles_CreatedBy')
ALTER TABLE auth.Profiles ADD CONSTRAINT FK_Profiles_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES auth.Users(UserId);
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Profiles_UpdatedBy')
ALTER TABLE auth.Profiles ADD CONSTRAINT FK_Profiles_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES auth.Users(UserId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_UserRoles_CreatedBy')
ALTER TABLE auth.UserRoles ADD CONSTRAINT FK_UserRoles_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES auth.Users(UserId);
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_UserRoles_UpdatedBy')
ALTER TABLE auth.UserRoles ADD CONSTRAINT FK_UserRoles_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES auth.Users(UserId);
GO

PRINT 'Audit foreign keys verified.';
GO

-- ============================================================================
-- 9. Helpful indexes
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Persons_LastName_FirstName' AND object_id = OBJECT_ID(N'auth.Persons'))
CREATE INDEX IX_Persons_LastName_FirstName ON auth.Persons(LastName, FirstName);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Users_IsActive' AND object_id = OBJECT_ID(N'auth.Users'))
CREATE INDEX IX_Users_IsActive ON auth.Users(IsActive) INCLUDE (Username);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Profiles_Department' AND object_id = OBJECT_ID(N'auth.Profiles'))
CREATE INDEX IX_Profiles_Department ON auth.Profiles(Department);
GO

PRINT 'Indexes verified.';
GO

-- ============================================================================
-- 10. Seed data (idempotent via MERGE / NOT EXISTS)
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM auth.DocumentTypes WHERE Code = 'DNI')
    INSERT INTO auth.DocumentTypes (Code, Name) VALUES ('DNI', 'National ID');

IF NOT EXISTS (SELECT 1 FROM auth.DocumentTypes WHERE Code = 'PASS')
    INSERT INTO auth.DocumentTypes (Code, Name) VALUES ('PASS', 'Passport');

IF NOT EXISTS (SELECT 1 FROM auth.DocumentTypes WHERE Code = 'CE')
    INSERT INTO auth.DocumentTypes (Code, Name) VALUES ('CE', 'Foreign Resident Card');
GO

IF NOT EXISTS (SELECT 1 FROM auth.Roles WHERE Name = 'Admin')
    INSERT INTO auth.Roles (Name, Description) VALUES ('Admin', 'System administrator - full access');

IF NOT EXISTS (SELECT 1 FROM auth.Roles WHERE Name = 'HR')
    INSERT INTO auth.Roles (Name, Description) VALUES ('HR', 'Human Resources - can create employee accounts');

IF NOT EXISTS (SELECT 1 FROM auth.Roles WHERE Name = 'Employee')
    INSERT INTO auth.Roles (Name, Description) VALUES ('Employee', 'Regular employee - can book medical appointments');
GO

PRINT 'Seed data verified.';
GO

-- ============================================================================
-- Done
-- ============================================================================
PRINT '====================================================';
PRINT ' Authentication module installed successfully.';
PRINT '====================================================';
GO