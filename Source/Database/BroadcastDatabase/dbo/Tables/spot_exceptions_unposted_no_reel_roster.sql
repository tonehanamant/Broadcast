CREATE TABLE [dbo].[spot_exceptions_unposted_no_reel_roster]
(
	[id] INT IDENTITY(1,1) PRIMARY KEY, 
    [house_isci] VARCHAR(50) NOT NULL, 
    [count] INT NOT NULL, 
    [program_air_time] DATETIME NOT NULL, 
    [estimate_id] BIGINT NOT NULL, 
    [ingested_by] VARCHAR(100) NOT NULL, 
    [ingested_at] DATETIME NOT NULL
)
