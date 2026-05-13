USE [ccs_db];
GO

MERGE ops.Authority AS tgt
USING (VALUES
    ('Policia Nacional - Linea 123', 'Police', 'Nacional', 'https://api.policia.gov.co/emergency', '+57123', 'kv-secret-policia'),
    ('Bomberos Bogota', 'FireDept', 'Bogota', 'https://api.bomberos.gov.co/alert', '+57119', 'kv-secret-bomberos'),
    ('Cruz Roja Colombiana', 'Ambulance', 'Nacional', 'https://api.cruzroja.org.co/alert', '+57132', 'kv-secret-cruzroja'),
    ('Transito Distrital', 'Transit', 'Bogota', 'https://api.transito.gov.co/event', '+57127', 'kv-secret-transito')
) AS src([Name], [Type], [Region], [WebhookUrl], [PhoneE164], [SecretKeyVaultRef])
ON tgt.[Name] = src.[Name]
WHEN NOT MATCHED THEN
    INSERT (AuthorityId, [Name], [Type], Region, WebhookUrl, PhoneE164, SecretKeyVaultRef)
    VALUES (NEWID(), src.[Name], src.[Type], src.Region, src.WebhookUrl, src.PhoneE164, src.SecretKeyVaultRef);
GO

