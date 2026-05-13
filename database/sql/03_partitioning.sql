USE [ccs_db];
GO

IF NOT EXISTS (SELECT 1 FROM sys.partition_functions WHERE name = 'pf_Monthly')
BEGIN
    CREATE PARTITION FUNCTION pf_Monthly (DATETIME2(3))
    AS RANGE RIGHT FOR VALUES (
        '2026-01-01','2026-02-01','2026-03-01','2026-04-01','2026-05-01','2026-06-01',
        '2026-07-01','2026-08-01','2026-09-01','2026-10-01','2026-11-01','2026-12-01',
        '2027-01-01'
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.partition_schemes WHERE name = 'ps_Monthly')
BEGIN
    CREATE PARTITION SCHEME ps_Monthly AS PARTITION pf_Monthly ALL TO ([PRIMARY]);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'CIX_NotificationLog_Part')
BEGIN
    ALTER TABLE ops.NotificationLog DROP CONSTRAINT PK_NotificationLog;
    ALTER TABLE ops.NotificationLog ADD CONSTRAINT PK_NotificationLog PRIMARY KEY NONCLUSTERED (NotificationId, SentAt);
    CREATE CLUSTERED INDEX CIX_NotificationLog_Part ON ops.NotificationLog (SentAt) ON ps_Monthly(SentAt);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'CIX_AuditLog_Part')
BEGIN
    ALTER TABLE audit.AuditLog DROP CONSTRAINT PK_AuditLog;
    ALTER TABLE audit.AuditLog ADD CONSTRAINT PK_AuditLog PRIMARY KEY NONCLUSTERED (AuditId, ChangedAt);
    CREATE CLUSTERED INDEX CIX_AuditLog_Part ON audit.AuditLog (ChangedAt) ON ps_Monthly(ChangedAt);
END;
GO

CREATE OR ALTER PROCEDURE ops.usp_AddNextMonthPartition
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @nextBoundary DATETIME2(3) =
        DATEADD(MONTH, 1, (
            SELECT MAX(CAST(prv.value AS DATETIME2(3)))
            FROM sys.partition_range_values prv
            JOIN sys.partition_functions pf ON pf.function_id = prv.function_id
            WHERE pf.name = 'pf_Monthly'
        ));

    ALTER PARTITION SCHEME ps_Monthly NEXT USED [PRIMARY];

    DECLARE @sql NVARCHAR(MAX) =
        N'ALTER PARTITION FUNCTION pf_Monthly() SPLIT RANGE (''' +
        CONVERT(NVARCHAR(30), @nextBoundary, 126) + N''')';

    EXEC sp_executesql @sql;
END;
GO

