﻿CREATE TABLE [dbo].[spot_exceptions_out_of_specs]
(
	[id] INT IDENTITY(1,1) PRIMARY KEY,
	[reason_code] VARCHAR(20) NOT NULL, 
	[reason_code_message] NVARCHAR(500) NULL, 
	[estimate_id] INT NOT NULL,
	[isci_name] VARCHAR(100) NOT NULL,
	[recommended_plan_id] INT NULL,
	[program_name] NVARCHAR(500) NULL,
	[station_legacy_call_letters] VARCHAR(15) NULL,
	[spot_lenth_id] INT NULL,
	[audience_id] INT NULL,
	[product] NVARCHAR(100) NULL,
	[flight_start_date] DATETIME NULL,
	[flight_end_date] DATETIME NULL,
	[daypart_id] INT NULL,
	[program_daypart_id] INT NOT NULL,
	[program_flight_start_date] DATETIME NOT NULL,
	[program_flight_end_date] DATETIME NOT NULL,
	[program_network] VARCHAR(10),
	[program_audience_id] INT NULL,
	[program_air_time] DATETIME NOT NULL,
	CONSTRAINT [FK_spot_exceptions_out_of_specs_plans] FOREIGN KEY ([recommended_plan_id]) REFERENCES [dbo].[plans]([ID]),
	CONSTRAINT [FK_spot_exceptions_out_of_specs_spot_lengths] FOREIGN KEY ([spot_lenth_id]) REFERENCES [dbo].[spot_lengths]([ID]),
	CONSTRAINT [FK_spot_exceptions_out_of_specs_audiences] FOREIGN KEY ([audience_id]) REFERENCES [dbo].[audiences]([ID]),
	CONSTRAINT [FK_spot_exceptions_out_of_specs_dayparts] FOREIGN KEY ([daypart_id]) REFERENCES [dbo].[dayparts]([ID]),
	CONSTRAINT [FK_spot_exceptions_out_of_specs_dayparts_program] FOREIGN KEY ([program_daypart_id]) REFERENCES [dbo].[dayparts]([ID]),
	CONSTRAINT [FK_spot_exceptions_out_of_specs_audiences_program] FOREIGN KEY ([program_audience_id]) REFERENCES [dbo].[audiences]([ID])		
)