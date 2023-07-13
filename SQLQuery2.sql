
--For Adding columns Synced_by and synced_at in spot_exceptions_recommended_plan_decisions Table

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'spot_exceptions_recommended_plan_decision' AND COLUMN_NAME= 'synced_by' AND COLUMN_NAME='synced_at')
BEGIN
	ALTER TABLE spot_exceptions_recommended_plan_decision
	ADD synced_by VARCHAR(100) NULL,
	synced_at DATETIME2 NULL
END

select * from spot_exceptions_unposted_no_reel_roster


--For Adding columns Synced_by and synced_at in spot_exceptions_out_of_spec_decisions Table

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'spot_exceptions_out_of_spec_decisions' AND COLUMN_NAME= 'synced_by' AND COLUMN_NAME='synced_at')
BEGIN
	ALTER TABLE spot_exceptions_out_of_spec_decisions
	ADD synced_by VARCHAR(100) NULL,
	synced_at DATETIME2 NULL
END



--Create table spot_exceptions_unposted_no_plan
IF OBJECT_ID('spot_exceptions_unposted_no_plan') IS NULL
BEGIN
	CREATE TABLE [dbo].spot_exceptions_unposted_no_plan
    (
	id INT IDENTITY(1,1) PRIMARY KEY,
	house_isci VARCHAR(50) NOT NULL,
	client_isci VARCHAR(50) NOT NULL,
	count INT NOT NULL,	
	program_air_time DATETIME NOT NULL,	
	estimate_id BIGINT NOT NULL,	
	ingested_by VARCHAR(100) NOT NULL,
	ingested_at DATETIME NOT NULL
    )
END


--Create table spot_exceptions_unposted_no_reel_roster

IF OBJECT_ID('spot_exceptions_unposted_no_reel_roster') IS NULL
BEGIN
    CREATE TABLE [dbo].spot_exceptions_unposted_no_reel_roster
     (
	id INT IDENTITY(1,1) PRIMARY KEY,
	house_isci VARCHAR(50) NOT NULL,
	count INT NOT NULL,	
	program_air_time DATETIME NOT NULL,	
	estimate_id BIGINT NOT NULL,
	ingested_by VARCHAR(100) NOT NULL,
	ingested_at DATETIME NOT NULL
    )
END