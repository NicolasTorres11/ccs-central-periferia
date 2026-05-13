USE [ccs_db];
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Vehicle_OwnerId')
    CREATE NONCLUSTERED INDEX IX_Vehicle_OwnerId ON fleet.Vehicle(OwnerId) INCLUDE (VehicleType, Status, Plate);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Vehicle_Active')
    CREATE NONCLUSTERED INDEX IX_Vehicle_Active ON fleet.Vehicle(Status) WHERE Status = 'Active';

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Device_LastSeen')
    CREATE NONCLUSTERED INDEX IX_Device_LastSeen ON fleet.Device(LastSeenAt DESC) WHERE Status = 'Active';

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Rule_Owner_Active')
    CREATE NONCLUSTERED INDEX IX_Rule_Owner_Active ON rules.[Rule](OwnerId, IsActive)
    INCLUDE (VehicleId, EventType, Priority, ConditionJson);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Rule_Vehicle_Event')
    CREATE NONCLUSTERED INDEX IX_Rule_Vehicle_Event ON rules.[Rule](VehicleId, EventType, IsActive)
    WHERE IsActive = 1;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_RuleAction_Rule')
    CREATE NONCLUSTERED INDEX IX_RuleAction_Rule ON rules.RuleAction(RuleId, [Order]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_NotificationLog_Correlation')
    CREATE NONCLUSTERED INDEX IX_NotificationLog_Correlation ON ops.NotificationLog(CorrelationId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_NotificationLog_Vehicle_Date')
    CREATE NONCLUSTERED INDEX IX_NotificationLog_Vehicle_Date ON ops.NotificationLog(VehicleId, SentAt DESC)
    INCLUDE (ChannelType, Status, LatencyMs);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AuditLog_Entity')
    CREATE NONCLUSTERED INDEX IX_AuditLog_Entity ON audit.AuditLog(EntityType, EntityId, ChangedAt DESC);
GO

