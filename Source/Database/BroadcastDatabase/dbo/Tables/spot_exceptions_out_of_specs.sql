﻿CREATE TABLE [dbo].[spot_exceptions_out_of_specs]
(
	[id] INT IDENTITY(1,1) PRIMARY KEY,	 
	[spot_unique_hash_external] [varchar](255) NOT NULL,
	[execution_id_external] VARCHAR(100) NOT NULL,
	[reason_code_message] NVARCHAR(500) NULL, 
	[estimate_id] INT NOT NULL,
	[isci_name] VARCHAR(100) NOT NULL,
	[recommended_plan_id] INT NULL,
	[program_name] NVARCHAR(500) NULL,
	[station_legacy_call_letters] VARCHAR(15) NULL,
	[spot_length_id] INT NULL,
	[audience_id] INT NULL,
	[product] NVARCHAR(100) NULL,
	[flight_start_date] DATETIME NULL,
	[flight_end_date] DATETIME NULL,
	[daypart_id] INT NULL,
	[program_network] VARCHAR(10),
	[program_air_time] DATETIME NOT NULL,	
    [advertiser_name] NVARCHAR(100) NULL,
	[reason_code_id] INT NOT NULL,	
    [impressions] FLOAT NOT NULL, 
    [market_code] INT NULL, 
    [market_rank] INT NULL, 
	[ingested_by] VARCHAR(100) NOT NULL, 
    [ingested_at] DATETIME NOT NULL, 
	[created_by] VARCHAR(100) NOT NULL,
	[created_at] DATETIME NOT NULL,
	[modified_by] VARCHAR(100) NOT NULL,
	[modified_at] DATETIME NOT NULL,
	[program_genre_id] [int] NULL,
	[house_isci] [varchar](100) NULL,
    CONSTRAINT [FK_spot_exceptions_out_of_specs_plans] FOREIGN KEY ([recommended_plan_id]) REFERENCES [dbo].[plans]([ID]),
	CONSTRAINT [FK_spot_exceptions_out_of_specs_spot_lengths] FOREIGN KEY ([spot_length_id]) REFERENCES [dbo].[spot_lengths]([ID]),
	CONSTRAINT [FK_spot_exceptions_out_of_specs_audiences] FOREIGN KEY ([audience_id]) REFERENCES [dbo].[audiences]([ID]),
	CONSTRAINT [FK_spot_exceptions_out_of_specs_dayparts] FOREIGN KEY ([daypart_id]) REFERENCES [dbo].[dayparts]([ID]),
	CONSTRAINT [FK_spot_exceptions_out_of_specs_program_genre] FOREIGN KEY ([program_genre_id]) REFERENCES [dbo].[genres]([ID]),
	CONSTRAINT [FK_spot_exceptions_out_of_specs_spot_exceptions_out_of_spec_reason_codes] FOREIGN KEY (reason_code_id) REFERENCES spot_exceptions_out_of_spec_reason_codes(id)
)