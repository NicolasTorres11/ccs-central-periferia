USE [ccs_db];
GO

IF DATABASE_PRINCIPAL_ID('ccs_app_writer') IS NULL CREATE ROLE ccs_app_writer;
IF DATABASE_PRINCIPAL_ID('ccs_app_reader') IS NULL CREATE ROLE ccs_app_reader;
GO

GRANT SELECT, INSERT, UPDATE ON SCHEMA::fleet TO ccs_app_writer;
GRANT SELECT, INSERT, UPDATE ON SCHEMA::rules TO ccs_app_writer;
GRANT INSERT ON SCHEMA::audit TO ccs_app_writer;
GRANT SELECT, INSERT, UPDATE ON SCHEMA::ops TO ccs_app_writer;

GRANT SELECT ON SCHEMA::fleet TO ccs_app_reader;
GRANT SELECT ON SCHEMA::rules TO ccs_app_reader;
GRANT SELECT ON SCHEMA::ops TO ccs_app_reader;
GO

CREATE OR ALTER FUNCTION rules.fn_OwnerPredicate(@OwnerId UNIQUEIDENTIFIER)
RETURNS TABLE WITH SCHEMABINDING AS
RETURN
    SELECT 1 AS allowed
    WHERE @OwnerId = TRY_CAST(SESSION_CONTEXT(N'OwnerId') AS UNIQUEIDENTIFIER)
       OR IS_ROLEMEMBER('db_owner') = 1;
GO

IF NOT EXISTS (SELECT 1 FROM sys.security_policies WHERE name = 'sp_OwnerIsolation')
BEGIN
    CREATE SECURITY POLICY rules.sp_OwnerIsolation
        ADD FILTER PREDICATE rules.fn_OwnerPredicate(OwnerId) ON fleet.Vehicle,
        ADD FILTER PREDICATE rules.fn_OwnerPredicate(OwnerId) ON rules.[Rule],
        ADD FILTER PREDICATE rules.fn_OwnerPredicate(OwnerId) ON fleet.EmergencyContact
    WITH (STATE = ON);
END;
GO

