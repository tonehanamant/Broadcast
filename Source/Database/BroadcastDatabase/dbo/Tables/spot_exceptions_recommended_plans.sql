CREATE TABLE [dbo].[spot_exceptions_recommended_plans]
(
	[id] INT NOT NULL PRIMARY KEY IDENTITY (1, 1),
	[estimate_id] INT NOT NULL,
	[isci_name] VARCHAR(50) NOT NULL,
	[recommended_plan_id] INT NULL ,
	[program_name] NVARCHAR(500) NULL,
	[program_air_time] DATETIME NOT NULL,	
	[station_legacy_call_letters] VARCHAR(15) NULL,
	[cost] MONEY NULL,
	[impressions] FLOAT NULL,
	[spot_length_id] INT NULL,
	[audience_id] INT NULL,
	[product] NVARCHAR(100) NULL,
	[flight_start_date] DATETIME NULL,
	[flight_end_date] DATETIME NULL,
	[daypart_id] INT NULL,
	[ingested_by] VARCHAR(100) NOT NULL, 
    [ingested_at] DATETIME NOT NULL, 
    CONSTRAINT [FK_spot_exceptions_recommended_plans_plans] FOREIGN KEY ([recommended_plan_id]) REFERENCES [dbo].[plans]([ID]),
	CONSTRAINT [FK_spot_exceptions_recommended_plans_spot_lengths] FOREIGN KEY ([spot_length_id]) REFERENCES [dbo].[spot_lengths]([ID]),
	CONSTRAINT [FK_spot_exceptions_recommended_plans_audiences] FOREIGN KEY ([audience_id]) REFERENCES [dbo].[audiences]([ID]),
	CONSTRAINT [FK_spot_exceptions_recommended_plans_dayparts] FOREIGN KEY ([daypart_id]) REFERENCES [dbo].[dayparts]([ID])
)