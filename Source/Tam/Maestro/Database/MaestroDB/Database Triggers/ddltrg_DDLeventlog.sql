
CREATE TRIGGER [ddltrg_DDLeventlog] ON DATABASE -- Create Database DDL Trigger
FOR DDL_DATABASE_LEVEL_EVENTS 
 -- the trigger will be executed for all the operations 
AS

SET NOCOUNT ON 

DECLARE @xmlEventData XML 

-- Capture the event data that is created 
SET @xmlEventData = eventdata() 

-- Insert information to a EventLog table
INSERT INTO dba.dbo.DDLeventlog(
	EventTime, EventType, ServerName,DatabaseName, ObjectType, ObjectName,
	UserName, LoginName, CommandText)
SELECT REPLACE(CONVERT(VARCHAR(50), 
@xmlEventData.query('data(/EVENT_INSTANCE/PostTime)')),'T', ' '),
CONVERT(VARCHAR(50), @xmlEventData.query('data(/EVENT_INSTANCE/EventType)')),
CONVERT(VARCHAR(25), @xmlEventData.query('data(/EVENT_INSTANCE/ServerName)')),
CONVERT(VARCHAR(25), @xmlEventData.query('data(/EVENT_INSTANCE/DatabaseName)')),
CONVERT(VARCHAR(25), @xmlEventData.query('data(/EVENT_INSTANCE/ObjectType)')),
CONVERT(VARCHAR(256), @xmlEventData.query('data(/EVENT_INSTANCE/ObjectName)')),
CONVERT(VARCHAR(256), @xmlEventData.query('data(/EVENT_INSTANCE/UserName)')),
CONVERT(VARCHAR(256), @xmlEventData.query('data(/EVENT_INSTANCE/LOGINNAME)')),
CONVERT(VARCHAR(MAX), @xmlEventData.query('data(/EVENT_INSTANCE/TSQLCommand/CommandText)')) 
--cast(@xmlEventData.query('data(/EVENT_INSTANCE/TSQLCommand/CommandText)') as VARCHAR(MAX)) 



GO
DISABLE TRIGGER [ddltrg_DDLeventlog]
    ON DATABASE;

