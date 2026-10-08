IF DB_ID(N'CommerceDB') IS NULL
BEGIN
    EXEC(N'CREATE DATABASE [CommerceDB]');
END;
GO

USE [CommerceDB];
GO

IF TYPE_ID(N'dbo.CommerceTableType') IS NULL
BEGIN
    EXEC(N'
        CREATE TYPE dbo.CommerceTableType AS TABLE
        (
            pc_processdate DATE NULL,
            pc_nomcomred NVARCHAR(200) NULL,
            pc_numdoc NVARCHAR(50) NULL
        );
    ');
END;
GO

IF OBJECT_ID(N'dbo.commerce', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.commerce
    (
        id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        pc_processdate DATE NULL,
        pc_nomcomred NVARCHAR(200) NULL,
        pc_numdoc NVARCHAR(50) NULL
    );
END;
GO

IF OBJECT_ID(N'dbo.commerce_quarantine', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.commerce_quarantine
    (
        id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        pc_processdate DATE NULL,
        pc_nomcomred NVARCHAR(200) NULL,
        pc_numdoc NVARCHAR(50) NULL,
        motivo NVARCHAR(500) NULL
    );
END;
GO

IF OBJECT_ID(N'dbo.commerce_imports', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.commerce_imports
    (
        id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        file_name NVARCHAR(260) NOT NULL,
        file_hash CHAR(64) NOT NULL UNIQUE,
        record_count INT NOT NULL,
        created_at DATETIME2(7) NOT NULL
            CONSTRAINT DF_commerce_imports_created_at DEFAULT SYSUTCDATETIME()
    );
END;
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE dbo.sp_create_commerce
    @registros dbo.CommerceTableType READONLY
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.commerce
    (
        pc_processdate,
        pc_nomcomred,
        pc_numdoc
    )
    SELECT
        pc_processdate,
        pc_nomcomred,
        pc_numdoc
    FROM @registros;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_process_commerce
    @fecha DATE
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @cantidad INT;

    BEGIN TRY
        BEGIN TRANSACTION;

        DELETE FROM dbo.commerce
        OUTPUT
            DELETED.pc_processdate,
            DELETED.pc_nomcomred,
            DELETED.pc_numdoc,
            CONCAT(
                CASE
                    WHEN NULLIF(TRIM(DELETED.pc_nomcomred), '') IS NULL
                    THEN N'Nombre del comercio vacío. '
                    ELSE N''
                END,
                CASE
                    WHEN NULLIF(TRIM(DELETED.pc_numdoc), '') IS NULL
                    THEN N'Número de documento vacío.'
                    WHEN DELETED.pc_numdoc COLLATE Latin1_General_BIN2 LIKE '%[^0-9]%'
                    THEN N'Número de documento contiene letras o caracteres especiales.'
                    ELSE N''
                END
            )
        INTO dbo.commerce_quarantine
        (
            pc_processdate,
            pc_nomcomred,
            pc_numdoc,
            motivo
        )
        WHERE pc_processdate = @fecha
          AND (
              NULLIF(TRIM(pc_nomcomred), '') IS NULL
              OR NULLIF(TRIM(pc_numdoc), '') IS NULL
              OR pc_numdoc COLLATE Latin1_General_BIN2 LIKE '%[^0-9]%'
          );

        SET @cantidad = @@ROWCOUNT;

        COMMIT TRANSACTION;

        SELECT @cantidad AS quarantinedCount;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END;
GO
