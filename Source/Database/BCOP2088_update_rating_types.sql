
/***************** TYPES ********************************/

IF EXISTS (SELECT 1 FROM sys.types WHERE is_table_type = 1 AND name = 'PointInTimeRatingsInput')
BEGIN
	DROP TYPE PointInTimeRatingsInput;
END
GO
IF EXISTS (SELECT 1 FROM sys.types WHERE is_table_type = 1 AND name = 'RatingsInput')
BEGIN
	DROP TYPE RatingsInput;
END
GO
IF EXISTS (SELECT 1 FROM sys.types WHERE is_table_type = 1 AND name = 'RatingsInputWithId')
BEGIN
	DROP TYPE RatingsInputWithId;
END
GO



CREATE TYPE [dbo].[PointInTimeRatingsInput] AS TABLE
(
	[id] [int] NOT NULL,
	[legacy_call_letters] varchar(15) NOT NULL,
	[start_time] [int] NOT NULL,
	PRIMARY KEY CLUSTERED 
	(
		[id] ASC,
		[legacy_call_letters] ASC,
		[start_time] ASC
	)WITH (IGNORE_DUP_KEY = OFF)
)

GO
CREATE TYPE [dbo].[RatingsInput] AS TABLE(
	[legacy_call_letters] varchar(15) NOT NULL,
	[mon] [bit] NOT NULL,
	[tue] [bit] NOT NULL,
	[wed] [bit] NOT NULL,
	[thu] [bit] NOT NULL,	
	[fri] [bit] NOT NULL,
	[sat] [bit] NOT NULL,
	[sun] [bit] NOT NULL,
	[start_time] [int] NOT NULL,
	[end_time] [int] NOT NULL,
	PRIMARY KEY CLUSTERED 
(
	[legacy_call_letters] ASC,
	[mon] ASC,
	[tue] ASC,
	[wed] ASC,
	[thu] ASC,
	[fri] ASC,
	[sat] ASC,
	[sun] ASC,
	[start_time] ASC,
	[end_time] ASC
)WITH (IGNORE_DUP_KEY = OFF)
)

GO

CREATE TYPE [dbo].[RatingsInputWithId] AS TABLE(
	[id] [int] NOT NULL,
	[legacy_call_letters] varchar(15) NOT NULL,
	[mon] [bit] NOT NULL,
	[tue] [bit] NOT NULL,
	[wed] [bit] NOT NULL,
	[thu] [bit] NOT NULL,
	[fri] [bit] NOT NULL,
	[sat] [bit] NOT NULL,
	[sun] [bit] NOT NULL,
	[start_time] [int] NOT NULL,
	[end_time] [int] NOT NULL,
	PRIMARY KEY CLUSTERED 
(
	[id] ASC,
	[legacy_call_letters] ASC,
	[mon] ASC,
	[tue] ASC,
	[wed] ASC,
	[thu] ASC,
	[fri] ASC,
	[sat] ASC,
	[sun] ASC,
	[start_time] ASC,
	[end_time] ASC
)WITH (IGNORE_DUP_KEY = OFF)
)

GO



/**************** TYPES END *******************************/



