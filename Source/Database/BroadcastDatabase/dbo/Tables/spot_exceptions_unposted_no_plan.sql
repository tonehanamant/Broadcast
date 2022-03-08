﻿CREATE TABLE [dbo].[spot_exceptions_unposted_no_plan]
(
	[id] INT IDENTITY(1,1) PRIMARY KEY, 
    [house_isci] VARCHAR(50) NOT NULL, 
    [client_isci] VARCHAR(50) NOT NULL, 
    [client_spot_length_id] INT NULL,
    [count] INT NOT NULL, 
    [program_air_time] DATETIME NOT NULL, 
    [estimate_id] BIGINT NOT NULL, 
    [ingested_by] VARCHAR(100) NOT NULL, 
    [ingested_at] DATETIME NOT NULL,
	[created_by] VARCHAR(100) NOT NULL,
	[created_at] DATETIME NOT NULL,
	[modified_by] VARCHAR(100) NOT NULL,
	[modified_at] DATETIME NOT NULL
)
