USE [ccs_db];
GO

IF SCHEMA_ID('fleet') IS NULL EXEC('CREATE SCHEMA fleet');
IF SCHEMA_ID('rules') IS NULL EXEC('CREATE SCHEMA rules');
IF SCHEMA_ID('audit') IS NULL EXEC('CREATE SCHEMA audit');
IF SCHEMA_ID('ops') IS NULL EXEC('CREATE SCHEMA ops');
GO

IF OBJECT_ID('fleet.Owner', 'U') IS NULL
BEGIN
    CREATE TABLE fleet.Owner (
        OwnerId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Owner PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        DocumentType NVARCHAR(20) NOT NULL,
        DocumentNumber NVARCHAR(50) NOT NULL,
        FullName NVARCHAR(200) NOT NULL,
        Email NVARCHAR(255) NULL,
        PhoneE164 NVARCHAR(20) NULL,
        Status NVARCHAR(20) NOT NULL DEFAULT 'Active',
        CreatedAt DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2(3) NULL,
        RowVersion ROWVERSION NOT NULL,
        CONSTRAINT UQ_Owner_Document UNIQUE (DocumentType, DocumentNumber),
        CONSTRAINT CK_Owner_Status CHECK (Status IN ('Active','Suspended','Cancelled'))
    );
END;
GO

IF OBJECT_ID('fleet.Vehicle', 'U') IS NULL
BEGIN
    CREATE TABLE fleet.Vehicle (
        VehicleId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Vehicle PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        OwnerId UNIQUEIDENTIFIER NOT NULL,
        VehicleType NVARCHAR(20) NOT NULL,
        Plate NVARCHAR(20) NOT NULL,
        Brand NVARCHAR(50) NULL,
        Model NVARCHAR(50) NULL,
        [Year] INT NULL,
        DeviceId NVARCHAR(100) NOT NULL,
        Status NVARCHAR(20) NOT NULL DEFAULT 'Active',
        EnrolledAt DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
        RowVersion ROWVERSION NOT NULL,
        CONSTRAINT FK_Vehicle_Owner FOREIGN KEY (OwnerId) REFERENCES fleet.Owner(OwnerId),
        CONSTRAINT UQ_Vehicle_Plate UNIQUE (Plate),
        CONSTRAINT UQ_Vehicle_Device UNIQUE (DeviceId),
        CONSTRAINT CK_Vehicle_Type CHECK (VehicleType IN ('Truck','Car','Motorcycle','Taxi','Bus')),
        CONSTRAINT CK_Vehicle_Status CHECK (Status IN ('Active','Maintenance','Inactive'))
    );
END;
GO

IF OBJECT_ID('fleet.Device', 'U') IS NULL
BEGIN
    CREATE TABLE fleet.Device (
        DeviceId NVARCHAR(100) NOT NULL CONSTRAINT PK_Device PRIMARY KEY,
        VehicleId UNIQUEIDENTIFIER NOT NULL,
        CertificateThumbprint NVARCHAR(100) NULL,
        FirmwareVersion NVARCHAR(50) NULL,
        LastSeenAt DATETIME2(3) NULL,
        IsCameraEnabled BIT NOT NULL DEFAULT 0,
        Status NVARCHAR(20) NOT NULL DEFAULT 'Provisioned',
        CONSTRAINT FK_Device_Vehicle FOREIGN KEY (VehicleId) REFERENCES fleet.Vehicle(VehicleId),
        CONSTRAINT UQ_Device_Vehicle UNIQUE (VehicleId),
        CONSTRAINT CK_Device_Status CHECK (Status IN ('Provisioned','Active','Decommissioned'))
    );
END;
GO

IF OBJECT_ID('rules.Rule', 'U') IS NULL
BEGIN
    CREATE TABLE rules.[Rule] (
        RuleId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Rule PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        OwnerId UNIQUEIDENTIFIER NOT NULL,
        VehicleId UNIQUEIDENTIFIER NULL,
        [Name] NVARCHAR(200) NOT NULL,
        EventType NVARCHAR(50) NOT NULL,
        ConditionJson NVARCHAR(MAX) NULL,
        Priority INT NOT NULL DEFAULT 3,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2(3) NULL,
        RowVersion ROWVERSION NOT NULL,
        CONSTRAINT FK_Rule_Owner FOREIGN KEY (OwnerId) REFERENCES fleet.Owner(OwnerId),
        CONSTRAINT FK_Rule_Vehicle FOREIGN KEY (VehicleId) REFERENCES fleet.Vehicle(VehicleId),
        CONSTRAINT CK_Rule_Event CHECK (EventType IN (
            'Panic','DriverDistress','UnplannedStop','OverSpeed','OutOfSchedule',
            'TempOutOfRange','Accident','GeofenceExit','GeofenceEnter')),
        CONSTRAINT CK_Rule_Priority CHECK (Priority BETWEEN 1 AND 5)
    );
END;
GO

IF OBJECT_ID('rules.RuleAction', 'U') IS NULL
BEGIN
    CREATE TABLE rules.RuleAction (
        RuleActionId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_RuleAction PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        RuleId UNIQUEIDENTIFIER NOT NULL,
        ActionType NVARCHAR(50) NOT NULL,
        TargetType NVARCHAR(50) NOT NULL,
        TargetReference NVARCHAR(500) NOT NULL,
        [Order] INT NOT NULL DEFAULT 1,
        IsCritical BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_RuleAction_Rule FOREIGN KEY (RuleId) REFERENCES rules.[Rule](RuleId) ON DELETE CASCADE,
        CONSTRAINT CK_RuleAction_Type CHECK (ActionType IN ('Sms','Email','Push','AuthorityCall','WebhookOwner','BroadcastDashboard')),
        CONSTRAINT CK_RuleAction_Target CHECK (TargetType IN ('Owner','Driver','Authority','Custom'))
    );
END;
GO

IF OBJECT_ID('fleet.EmergencyContact', 'U') IS NULL
BEGIN
    CREATE TABLE fleet.EmergencyContact (
        ContactId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_EmergencyContact PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        OwnerId UNIQUEIDENTIFIER NOT NULL,
        ContactName NVARCHAR(200) NOT NULL,
        Relationship NVARCHAR(50) NULL,
        PhoneE164 NVARCHAR(20) NULL,
        Email NVARCHAR(255) NULL,
        Priority INT NOT NULL DEFAULT 1,
        CONSTRAINT FK_EmergencyContact_Owner FOREIGN KEY (OwnerId) REFERENCES fleet.Owner(OwnerId)
    );
END;
GO

IF OBJECT_ID('ops.Authority', 'U') IS NULL
BEGIN
    CREATE TABLE ops.Authority (
        AuthorityId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Authority PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        [Name] NVARCHAR(200) NOT NULL,
        [Type] NVARCHAR(50) NOT NULL,
        Region NVARCHAR(100) NULL,
        WebhookUrl NVARCHAR(500) NULL,
        PhoneE164 NVARCHAR(20) NULL,
        SecretKeyVaultRef NVARCHAR(200) NULL,
        CONSTRAINT CK_Authority_Type CHECK ([Type] IN ('Police','FireDept','Ambulance','Transit','Rescue'))
    );
END;
GO

IF OBJECT_ID('audit.AuditLog', 'U') IS NULL
BEGIN
    CREATE TABLE audit.AuditLog (
        AuditId BIGINT IDENTITY(1,1) NOT NULL,
        EntityType NVARCHAR(50) NOT NULL,
        EntityId NVARCHAR(100) NOT NULL,
        [Action] NVARCHAR(20) NOT NULL,
        ChangedBy NVARCHAR(200) NULL,
        ChangedAt DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
        OldValueJson NVARCHAR(MAX) NULL,
        NewValueJson NVARCHAR(MAX) NULL,
        CorrelationId UNIQUEIDENTIFIER NULL,
        CONSTRAINT PK_AuditLog PRIMARY KEY (AuditId, ChangedAt),
        CONSTRAINT CK_AuditLog_Action CHECK ([Action] IN ('Created','Updated','Deleted'))
    );
END;
GO

IF OBJECT_ID('ops.NotificationLog', 'U') IS NULL
BEGIN
    CREATE TABLE ops.NotificationLog (
        NotificationId UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
        CorrelationId UNIQUEIDENTIFIER NOT NULL,
        RuleId UNIQUEIDENTIFIER NULL,
        VehicleId UNIQUEIDENTIFIER NULL,
        ChannelType NVARCHAR(50) NOT NULL,
        Target NVARCHAR(500) NULL,
        Status NVARCHAR(20) NOT NULL,
        LatencyMs INT NULL,
        SentAt DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
        ErrorDetail NVARCHAR(MAX) NULL,
        CONSTRAINT PK_NotificationLog PRIMARY KEY (NotificationId, SentAt),
        CONSTRAINT FK_NotificationLog_Rule FOREIGN KEY (RuleId) REFERENCES rules.[Rule](RuleId),
        CONSTRAINT FK_NotificationLog_Vehicle FOREIGN KEY (VehicleId) REFERENCES fleet.Vehicle(VehicleId),
        CONSTRAINT CK_Notification_Status CHECK (Status IN ('Sent','Failed','Retrying','Skipped'))
    );
END;
GO

