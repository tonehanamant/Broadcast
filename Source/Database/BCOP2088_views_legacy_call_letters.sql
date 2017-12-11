

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[nsi].[usp_GetImpressionsForMultiplePrograms_TwoBooks_Averages]') AND type in (N'P', N'PC'))
DROP PROCEDURE [nsi].[usp_GetImpressionsForMultiplePrograms_TwoBooks_Averages]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[nsi].[usp_GetImpressionsForMultiplePrograms_TwoBooks]') AND type in (N'P', N'PC'))
DROP PROCEDURE [nsi].[usp_GetImpressionsForMultiplePrograms_TwoBooks]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[nsi].[usp_GetImpressionsForMultiplePrograms_Daypart_Averages]') AND type in (N'P', N'PC'))
DROP PROCEDURE [nsi].[usp_GetImpressionsForMultiplePrograms_Daypart_Averages]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[nsi].[usp_GetImpressionsForMultiplePrograms_Daypart]') AND type in (N'P', N'PC'))
DROP PROCEDURE [nsi].[usp_GetImpressionsForMultiplePrograms_Daypart]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[nsi].[usp_ForecastNsiRatingsMonth]') AND type in (N'P', N'PC'))
DROP PROCEDURE [nsi].[usp_ForecastNsiRatingsMonth]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[nsi].[usp_ForecastNsiRatingsForMultiplePrograms_Averages]') AND type in (N'P', N'PC'))
DROP PROCEDURE [nsi].[usp_ForecastNsiRatingsForMultiplePrograms_Averages]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[nsi].[usp_ForecastNsiRatingsForMultiplePrograms]') AND type in (N'P', N'PC'))
DROP PROCEDURE [nsi].[usp_ForecastNsiRatingsForMultiplePrograms]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[nsi].[udf_GetForecastedNsiRatings]'))
DROP FUNCTION [nsi].[udf_GetForecastedNsiRatings]



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




IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'legacy_call_letters'
          AND Object_ID = Object_ID(N'nsi.viewers'))
BEGIN
	ALTER TABLE nsi.viewers 
	DROP CONSTRAINT [PK_nsi_viewers]

	ALTER TABLE nsi.viewers ADD legacy_call_letters varchar(15);
	exec('update nsi.viewers set legacy_call_letters = cast(station_code as varchar(15)) where legacy_call_letters is null')
	ALTER TABLE nsi.viewers ALTER COLUMN legacy_call_letters varchar(15) not null;

	ALTER TABLE nsi.viewers 
		ADD CONSTRAINT [PK_nsi_viewers] PRIMARY KEY 
		(
			[media_month_id] ASC,
			[legacy_call_letters] ASC,
			[start_time] ASC,
			[end_time] ASC,
			[market_code] ASC
		) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) 
				ON [MediaMonthSmallIntScheme]([media_month_id])

	ALTER TABLE	nsi.viewers DROP COLUMN STATION_CODE 
END
GO

IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'legacy_call_letters'
          AND Object_ID = Object_ID(N'nsi.viewers_trunc'))
BEGIN
	ALTER TABLE nsi.viewers_trunc 
	DROP CONSTRAINT [PK_nsi_viewers_trunc]

	ALTER TABLE nsi.viewers_trunc ADD legacy_call_letters varchar(15);
	exec('update nsi.viewers_trunc set legacy_call_letters = cast(station_code as varchar(15)) where legacy_call_letters is null')
	ALTER TABLE nsi.viewers_trunc ALTER COLUMN legacy_call_letters varchar(15) not null;

	ALTER TABLE nsi.viewers_trunc
		ADD CONSTRAINT [PK_nsi_viewers_trunc] PRIMARY KEY 
		(
			[media_month_id] ASC,
			[legacy_call_letters] ASC,
			[start_time] ASC,
			[end_time] ASC,
			[market_code] ASC
		) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) 

	ALTER TABLE	nsi.viewers_trunc DROP COLUMN STATION_CODE 

END
GO

IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'legacy_call_letters'
          AND Object_ID = Object_ID(N'nsi.viewers_arc'))
BEGIN
	ALTER TABLE nsi.viewers_arc 
	DROP CONSTRAINT [PK_nsi_viewers_arc]

	ALTER TABLE nsi.viewers_arc ADD legacy_call_letters varchar(15);
	exec('update nsi.viewers_arc set legacy_call_letters = cast(station_code as varchar(15)) where legacy_call_letters is null')
	ALTER TABLE nsi.viewers_arc ALTER COLUMN legacy_call_letters varchar(15) not null;

	ALTER TABLE nsi.viewers_arc
		ADD CONSTRAINT [PK_nsi_viewers_arc] PRIMARY KEY 
		(
			[media_month_id] ASC,
			[legacy_call_letters] ASC,
			[start_time] ASC,
			[end_time] ASC,
			[market_code] ASC
		) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) 

	ALTER TABLE	nsi.viewers_arc DROP COLUMN STATION_CODE 

END
GO


