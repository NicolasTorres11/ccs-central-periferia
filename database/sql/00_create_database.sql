-- Ejecutar contra master.
IF DB_ID(N'ccs_db') IS NULL
BEGIN
    CREATE DATABASE [ccs_db]
    (
        EDITION = 'BusinessCritical',
        SERVICE_OBJECTIVE = 'BC_Gen5_4',
        MAXSIZE = 256 GB
    );
END;
GO

ALTER DATABASE [ccs_db] SET RECOVERY FULL;
ALTER DATABASE [ccs_db] SET QUERY_STORE = ON;
ALTER DATABASE [ccs_db] SET ENCRYPTION ON;
GO

