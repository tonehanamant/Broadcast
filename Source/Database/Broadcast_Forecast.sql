---------------------------------------------------------------------------------------------------
-- !!! If Executing from SQL Server Manager, please enable SQLCMD Mode!!! To enable option, select menu Query->Enable SQLCMD mode. --
---------------------------------------------------------------------------------------------------
-- All scripts should be written in a way that they can be run multiple times
-- All features/bugs should be wrapped in comments indicating the start/end of the scripts
---------------------------------------------------------------------------------------------------
---------------------------------------------------------------------------------------------------
-- TFS Items:
---------------------------------------------------------------------------------------------------
---------------------------------------------------------------------------------------------------
SET NOEXEC OFF;
SET NOCOUNT OFF;
GO

:on error exit --sqlcmd exit script on error
GO
:setvar __IsSqlCmdEnabled "True"
GO
IF N'$(__IsSqlCmdEnabled)' NOT LIKE N'True'
    BEGIN
        PRINT N'SQLCMD mode must be enabled to successfully execute this script.To enable option, select menu Query->Enable SQLCMD mode';
		SET NOCOUNT ON;
        SET NOEXEC ON; -- this will not execute any queries. queries will be compiled only.
    END
GO

SET XACT_ABORT ON -- Rollback transaction incase of error
GO





BEGIN
	PRINT 'RUNNING SCRIPT IN LOCAL DATBASE'
END
GO


BEGIN TRANSACTION

CREATE TABLE #previous_version 
( 
	[version] VARCHAR(32) 
)
GO

-- Only run this script when the schema is in the correct pervious version
INSERT INTO #previous_version
		SELECT parameter_value 
		FROM system_component_parameters 
		WHERE parameter_key = 'SchemaVersion' 


/*************************************** START UPDATE SCRIPT *****************************************************/


/*************************************** START BCOP-2088 *********************************************************/




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




IF object_id('[nsi].[usp_ForecastNsiRatingsForMultiplePrograms]') IS NOT NULL
BEGIN
	DROP PROC [nsi].[usp_ForecastNsiRatingsForMultiplePrograms]
END

GO

-- =============================================
-- Author:		
-- Create date: 12/20/2016
-- Description:	
-- =============================================
/*
	DECLARE
		@hut_media_month_id SMALLINT = 401,
		@share_media_month_id SMALLINT = 404,
		@demo VARCHAR(MAX) = '13,14,15,347,348',
		@ratings_request RatingsInput,
		@min_playback_type VARCHAR(1) = '3'

	INSERT INTO @ratings_request SELECT 602,1,1,1,1,1,0,0,77400,79199
	
	EXEC [nsi].[usp_ForecastNsiRatingsForMultiplePrograms] @hut_media_month_id, @share_media_month_id, @demo, @ratings_request, @min_playback_type
*/
-- =============================================
CREATE PROCEDURE [nsi].[usp_ForecastNsiRatingsForMultiplePrograms]
	@hut_media_month_id SMALLINT,
	@share_media_month_id SMALLINT,
	@demo VARCHAR(MAX),
	@ratings_request RatingsInput READONLY,
	@min_playback_type VARCHAR(1)
AS
BEGIN
	SET NOCOUNT ON;
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

	DECLARE @audience_ids TABLE (audience_id INT NOT NULL, 
		PRIMARY KEY CLUSTERED(audience_id ASC));
	INSERT INTO @audience_ids
		SELECT id FROM dbo.SplitIntegers(@demo);
	
	DECLARE @hut_min_playback_types TABLE (market_code SMALLINT NOT NULL, available_playback_type VARCHAR(1), 
		PRIMARY KEY CLUSTERED(market_code ASC))
	INSERT INTO @hut_min_playback_types
		SELECT * FROM nsi.udf_GetMinPlaybackTypes(@hut_media_month_id,@min_playback_type);

	DECLARE @market_codes TABLE (legacy_call_letters varchar(15) NOT NULL, market_code INT NOT NULL, 
		PRIMARY KEY CLUSTERED(legacy_call_letters ASC, market_code ASC))
	INSERT INTO @market_codes
		SELECT
			rr.legacy_call_letters,
			v.market_code
		FROM
			@ratings_request rr
			JOIN nsi.viewers v (NOLOCK) ON @hut_media_month_id = v.media_month_id
				AND v.legacy_call_letters=rr.legacy_call_letters
				AND (v.start_time<=rr.end_time AND v.end_time>=rr.start_time)
		GROUP BY
			rr.legacy_call_letters,
			v.market_code;

	DECLARE @share_market_codes TABLE (legacy_call_letters varchar(15) NOT NULL, market_code INT NOT NULL, PRIMARY KEY CLUSTERED(legacy_call_letters ASC, market_code ASC))
	INSERT INTO @share_market_codes
		SELECT
			rr.legacy_call_letters,
			v.market_code
		FROM
			@ratings_request rr
			JOIN nsi.viewers v (NOLOCK) ON @share_media_month_id = v.media_month_id
				AND v.legacy_call_letters=rr.legacy_call_letters
				AND (v.start_time<=rr.end_time AND v.end_time>=rr.start_time)
		GROUP BY
			rr.legacy_call_letters,
			v.market_code;

	DECLARE @hut TABLE (legacy_call_letters varchar(15) NOT NULL, mon BIT NOT NULL, tue BIT NOT NULL, wed BIT NOT NULL, thu BIT NOT NULL, fri BIT NOT NULL, sat BIT NOT NULL, sun BIT NOT NULL, start_time INT NOT NULL, end_time INT NOT NULL, hut FLOAT NOT NULL 
		PRIMARY KEY CLUSTERED(legacy_call_letters ASC, mon ASC, tue ASC, wed ASC, thu ASC, fri ASC, sat ASC, sun ASC, start_time ASC, end_time ASC));
	INSERT INTO @hut
		SELECT
			legacy_call_letters,mon,tue,wed,thu,fri,sat,sun,start_time,end_time,
			SUM(mon_usage + tue_usage + wed_usage + thu_usage + fri_usage + sat_usage + sun_usage) / SUM(universe) 'hut'
		FROM (
			SELECT
				rr.legacy_call_letters,rr.mon,rr.tue,rr.wed,rr.thu,rr.fri,rr.sat,rr.sun,u.start_time,u.end_time,
				uv.universe * (
					CAST(rr.mon AS INT) +
					CAST(rr.tue AS INT) +
					CAST(rr.wed AS INT) +
					CAST(rr.thu AS INT) +
					CAST(rr.fri AS INT) +
					CAST(rr.sat AS INT) +
					CAST(rr.sun AS INT)) 'universe',
				CASE rr.mon WHEN 1 THEN u.mon_usage ELSE 0 END 'mon_usage',
				CASE rr.tue WHEN 1 THEN u.tue_usage ELSE 0 END 'tue_usage',
				CASE rr.wed WHEN 1 THEN u.wed_usage ELSE 0 END 'wed_usage',
				CASE rr.thu WHEN 1 THEN u.thu_usage ELSE 0 END 'thu_usage',
				CASE rr.fri WHEN 1 THEN u.fri_usage ELSE 0 END 'fri_usage',
				CASE rr.sat WHEN 1 THEN u.sat_usage ELSE 0 END 'sat_usage',
				CASE rr.sun WHEN 1 THEN u.sun_usage ELSE 0 END 'sun_usage'
			FROM
				@ratings_request rr
				JOIN @market_codes hdi ON hdi.legacy_call_letters=rr.legacy_call_letters
				JOIN @hut_min_playback_types mpt ON mpt.market_code=hdi.market_code
				CROSS APPLY @audience_ids a
				JOIN nsi.usages u (NOLOCK) ON u.media_month_id=@hut_media_month_id
					AND u.playback_type=mpt.available_playback_type
					AND u.audience_id=a.audience_id
					AND (u.start_time<=rr.end_time AND u.end_time>=rr.start_time)
					AND u.market_code=hdi.market_code
				JOIN nsi.universes uv (NOLOCK) ON uv.media_month_id=u.media_month_id
					AND uv.playback_type=mpt.available_playback_type
					AND uv.market_code=u.market_code
					AND uv.audience_id=a.audience_id
		) tmp
		GROUP BY
			legacy_call_letters,
			mon,tue,wed,thu,fri,sat,sun,
			start_time,end_time

	DECLARE @share_min_playback_types TABLE (market_code SMALLINT NOT NULL, available_playback_type VARCHAR(1), 
		PRIMARY KEY CLUSTERED(market_code ASC))
	INSERT INTO @share_min_playback_types
		SELECT * FROM nsi.udf_GetMinPlaybackTypes(@share_media_month_id,@min_playback_type);

	DECLARE @share_usages TABLE (legacy_call_letters varchar(15) NOT NULL, mon BIT NOT NULL, tue BIT NOT NULL, wed BIT NOT NULL, thu BIT NOT NULL, fri BIT NOT NULL, sat BIT NOT NULL, sun BIT NOT NULL, start_time INT NOT NULL, end_time INT NOT NULL, usage FLOAT NOT NULL 
		PRIMARY KEY CLUSTERED(legacy_call_letters ASC, mon ASC, tue ASC, wed ASC, thu ASC, fri ASC, sat ASC, sun ASC, start_time ASC, end_time ASC));
	INSERT INTO @share_usages
			SELECT
				rr.legacy_call_letters,rr.mon,rr.tue,rr.wed,rr.thu,rr.fri,rr.sat,rr.sun,u.start_time,u.end_time,
				SUM(CASE rr.mon WHEN 1 THEN u.mon_usage ELSE 0 END +
				CASE rr.tue WHEN 1 THEN u.tue_usage ELSE 0 END +
				CASE rr.wed WHEN 1 THEN u.wed_usage ELSE 0 END +
				CASE rr.thu WHEN 1 THEN u.thu_usage ELSE 0 END +
				CASE rr.fri WHEN 1 THEN u.fri_usage ELSE 0 END +
				CASE rr.sat WHEN 1 THEN u.sat_usage ELSE 0 END +
				CASE rr.sun WHEN 1 THEN u.sun_usage ELSE 0 END) 'usage'
			FROM
				@ratings_request rr
				JOIN @share_market_codes hdi ON hdi.legacy_call_letters=rr.legacy_call_letters
				JOIN @share_min_playback_types mpt ON mpt.market_code=hdi.market_code
				CROSS APPLY @audience_ids a
				JOIN nsi.usages u (NOLOCK) ON u.media_month_id=@share_media_month_id
					AND u.playback_type=mpt.available_playback_type
					AND u.audience_id=a.audience_id
					AND (u.start_time<=rr.end_time AND u.end_time>=rr.start_time)
					AND u.market_code=hdi.market_code
				JOIN nsi.universes uv (NOLOCK) ON uv.media_month_id=u.media_month_id
					AND uv.playback_type=mpt.available_playback_type
					AND uv.market_code=u.market_code
					AND uv.audience_id=a.audience_id
		GROUP BY
			rr.legacy_call_letters,
			mon,tue,wed,thu,fri,sat,sun,
			u.start_time,u.end_time

	DECLARE @share_viewers TABLE (legacy_call_letters varchar(15) NOT NULL, mon BIT NOT NULL, tue BIT NOT NULL, wed BIT NOT NULL, thu BIT NOT NULL, fri BIT NOT NULL, sat BIT NOT NULL, sun BIT NOT NULL, start_time INT NOT NULL, end_time INT NOT NULL, viewers FLOAT NOT NULL 
		PRIMARY KEY CLUSTERED(legacy_call_letters ASC, mon ASC, tue ASC, wed ASC, thu ASC, fri ASC, sat ASC, sun ASC, start_time ASC, end_time ASC));
	INSERT INTO @share_viewers
			SELECT
				rr.legacy_call_letters,rr.mon,rr.tue,rr.wed,rr.thu,rr.fri,rr.sat,rr.sun,v.start_time,v.end_time,
				SUM(CASE rr.mon WHEN 1 THEN vd.mon_viewers ELSE 0 END +
				CASE rr.tue WHEN 1 THEN vd.tue_viewers ELSE 0 END +
				CASE rr.wed WHEN 1 THEN vd.wed_viewers ELSE 0 END +
				CASE rr.thu WHEN 1 THEN vd.thu_viewers ELSE 0 END +
				CASE rr.fri WHEN 1 THEN vd.fri_viewers ELSE 0 END +
				CASE rr.sat WHEN 1 THEN vd.sat_viewers ELSE 0 END +
				CASE rr.sun WHEN 1 THEN vd.sun_viewers ELSE 0 END) 'viewers'
			FROM
				@ratings_request rr
				JOIN nsi.viewers v (NOLOCK) ON v.media_month_id=@share_media_month_id
					AND v.legacy_call_letters=rr.legacy_call_letters
					AND (v.start_time<=rr.end_time AND v.end_time>=rr.start_time)
				JOIN @share_min_playback_types mpt ON mpt.market_code=v.market_code
				CROSS APPLY @audience_ids a
				JOIN nsi.viewer_details vd (NOLOCK) ON vd.media_month_id=v.media_month_id
					AND vd.viewer_id=v.id
					AND vd.audience_id=a.audience_id
					AND vd.playback_type=mpt.available_playback_type
		GROUP BY
			rr.legacy_call_letters,mon,tue,wed,thu,fri,sat,sun,v.start_time,v.end_time

	DECLARE @share TABLE (legacy_call_letters varchar(15) NOT NULL, mon BIT NOT NULL, tue BIT NOT NULL, wed BIT NOT NULL, thu BIT NOT NULL, fri BIT NOT NULL, sat BIT NOT NULL, sun BIT NOT NULL, start_time INT NOT NULL, end_time INT NOT NULL, share FLOAT NOT NULL, 
		PRIMARY KEY CLUSTERED(legacy_call_letters ASC, mon ASC, tue ASC, wed ASC, thu ASC, fri ASC, sat ASC, sun ASC, start_time ASC, end_time ASC));
	INSERT INTO @share
		SELECT 
			v.legacy_call_letters,
			v.mon,
			v.tue,
			v.wed,
			v.thu,
			v.fri,
			v.sat,
			v.sun,
			v.start_time,
			v.end_time,
			CASE u.usage WHEN 0 THEN 0 ELSE v.viewers / u.usage END 'share'
		FROM 
			@share_viewers v
			JOIN @share_usages u ON u.legacy_call_letters = v.legacy_call_letters
				AND u.mon = v.mon
				AND u.tue = v.tue
				AND u.wed = v.wed
				AND u.thu = v.thu
				AND u.fri = v.fri
				AND u.sat = v.sat
				AND u.sun = v.sun
				AND u.start_time = v.start_time
				AND u.end_time = v.end_time;

	SELECT
		h.legacy_call_letters,h.mon,h.tue,h.wed,h.thu,h.fri,h.sat,h.sun,rr.start_time,rr.end_time,
		AVG(h.hut*s.share) 'rating'
	FROM
		@hut h
		JOIN @share s ON s.legacy_call_letters=h.legacy_call_letters
			AND s.mon=h.mon
			AND s.tue=h.tue
			AND s.wed=h.wed
			AND s.thu=h.thu
			AND s.fri=h.fri
			AND s.sat=h.sat
			AND s.sun=h.sun
			AND s.start_time=h.start_time
			AND s.end_time=h.end_time
		JOIN @ratings_request rr ON h.legacy_call_letters=rr.legacy_call_letters
			AND h.mon=rr.mon
			AND h.tue=rr.tue
			AND h.wed=rr.wed
			AND h.thu=rr.thu
			AND h.fri=rr.fri
			AND h.sat=rr.sat
			AND h.sun=rr.sun
			AND (h.start_time<=rr.end_time AND h.end_time>=rr.start_time)			
	GROUP BY
		h.legacy_call_letters,h.mon,h.tue,h.wed,h.thu,h.fri,h.sat,h.sun,rr.start_time,rr.end_time
END


GO

IF object_id('[nsi].[usp_ForecastNsiRatingsForMultiplePrograms_Averages]') IS NOT NULL
BEGIN
	DROP PROC [nsi].[usp_ForecastNsiRatingsForMultiplePrograms_Averages]
END

GO

/*
-- example exec
DECLARE
	@hut_media_month_id SMALLINT = 418,
	@share_media_month_id SMALLINT = 418,
	@demo VARCHAR(MAX) = '31',
	@ratings_request RatingsInput,
	@min_playback_type VARCHAR(1) = '1'

	
INSERT INTO @ratings_request SELECT 'WUNC',1,1,1,1,1,1,1,20000,66599


EXEC [nsi].[usp_ForecastNsiRatingsForMultiplePrograms_Averages]
		@hut_media_month_id,@share_media_month_id, @demo, @ratings_request, @min_playback_type
*/
CREATE PROCEDURE [nsi].[usp_ForecastNsiRatingsForMultiplePrograms_Averages]
	@hut_media_month_id SMALLINT,
	@share_media_month_id SMALLINT,
	@demo VARCHAR(MAX),
	@ratings_request RatingsInput READONLY,
	@min_playback_type VARCHAR(1)
AS
BEGIN

	SET NOCOUNT ON;
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

	DECLARE @audience_ids TABLE (audience_id INT NOT NULL, 
		PRIMARY KEY CLUSTERED(audience_id ASC));
	INSERT INTO @audience_ids
		SELECT id FROM dbo.SplitIntegers(@demo);
	
	DECLARE @hut_min_playback_types TABLE (market_code SMALLINT NOT NULL, available_playback_type VARCHAR(1), 
		PRIMARY KEY CLUSTERED(market_code ASC))
	INSERT INTO @hut_min_playback_types
		SELECT * FROM nsi.udf_GetMinPlaybackTypes(@hut_media_month_id,@min_playback_type);

	DECLARE @market_codes TABLE (legacy_call_letters varchar(15) NOT NULL, market_code INT NOT NULL, 
		PRIMARY KEY CLUSTERED(legacy_call_letters ASC, market_code ASC))
	INSERT INTO @market_codes
		SELECT
			rr.legacy_call_letters,
			v.market_code
		FROM
			@ratings_request rr
			JOIN nsi.viewers v (NOLOCK) ON @hut_media_month_id = v.media_month_id
				AND v.legacy_call_letters=rr.legacy_call_letters
				AND (v.start_time<=rr.end_time AND v.end_time>=rr.start_time)
		GROUP BY
			rr.legacy_call_letters,
			v.market_code;

	DECLARE @share_market_codes TABLE (legacy_call_letters varchar(15) NOT NULL, market_code INT NOT NULL, 
		PRIMARY KEY CLUSTERED(legacy_call_letters ASC, market_code ASC))
	INSERT INTO @share_market_codes
		SELECT
			rr.legacy_call_letters,
			v.market_code
		FROM
			@ratings_request rr
			JOIN nsi.viewers v (NOLOCK) ON @share_media_month_id = v.media_month_id
				AND v.legacy_call_letters=rr.legacy_call_letters
				AND (v.start_time<=rr.end_time AND v.end_time>=rr.start_time)
		GROUP BY
			rr.legacy_call_letters,
			v.market_code;

	DECLARE @hut TABLE (legacy_call_letters varchar(15) NOT NULL, mon BIT NOT NULL, tue BIT NOT NULL, wed BIT NOT NULL, thu BIT NOT NULL, fri BIT NOT NULL, sat BIT NOT NULL, sun BIT NOT NULL, start_time INT NOT NULL, end_time INT NOT NULL, hut FLOAT NOT NULL 
		PRIMARY KEY CLUSTERED(legacy_call_letters ASC, mon ASC, tue ASC, wed ASC, thu ASC, fri ASC, sat ASC, sun ASC, start_time ASC, end_time ASC));
	INSERT INTO @hut
		SELECT
			legacy_call_letters,mon,tue,wed,thu,fri,sat,sun,start_time,end_time,
			SUM(usage) / SUM(universe) 'hut'
		FROM (
			SELECT
				rr.legacy_call_letters,rr.mon,rr.tue,rr.wed,rr.thu,rr.fri,rr.sat,rr.sun,u.start_time,u.end_time,
				uv.universe * (
					CAST(rr.mon AS INT) +
					CAST(rr.tue AS INT) +
					CAST(rr.wed AS INT) +
					CAST(rr.thu AS INT) +
					CAST(rr.fri AS INT) +
					CAST(rr.sat AS INT) +
					CAST(rr.sun AS INT)) 'universe',
				CASE rr.mon WHEN 1 THEN u.weekday_usage ELSE 0 END +
				CASE rr.tue WHEN 1 THEN u.weekday_usage ELSE 0 END +
				CASE rr.wed WHEN 1 THEN u.weekday_usage ELSE 0 END +
				CASE rr.thu WHEN 1 THEN u.weekday_usage ELSE 0 END +
				CASE rr.fri WHEN 1 THEN u.weekday_usage ELSE 0 END +
				CASE rr.sat WHEN 1 THEN u.weekend_usage ELSE 0 END +
				CASE rr.sun WHEN 1 THEN u.weekend_usage ELSE 0 END 'usage'
			FROM
				@ratings_request rr
				JOIN @market_codes hdi ON hdi.legacy_call_letters=rr.legacy_call_letters
				JOIN @hut_min_playback_types mpt ON mpt.market_code=hdi.market_code
				CROSS APPLY @audience_ids a
				JOIN nsi.usages u (NOLOCK) ON u.media_month_id=@hut_media_month_id
					AND u.playback_type=mpt.available_playback_type
					AND u.audience_id=a.audience_id
					AND (u.start_time<=rr.end_time AND u.end_time>=rr.start_time)
					AND u.market_code=hdi.market_code
				JOIN nsi.universes uv (NOLOCK) ON uv.media_month_id=u.media_month_id
					AND uv.playback_type=mpt.available_playback_type
					AND uv.market_code=u.market_code
					AND uv.audience_id=a.audience_id
		) tmp
		GROUP BY
			legacy_call_letters,
			mon,tue,wed,thu,fri,sat,sun,
			start_time,end_time

	DECLARE @share_min_playback_types TABLE (market_code SMALLINT NOT NULL, available_playback_type VARCHAR(1), 
		PRIMARY KEY CLUSTERED(market_code ASC))
	INSERT INTO @share_min_playback_types
		SELECT * FROM nsi.udf_GetMinPlaybackTypes(@share_media_month_id,@min_playback_type);

	DECLARE @share_usages TABLE (legacy_call_letters varchar(15) NOT NULL, mon BIT NOT NULL, tue BIT NOT NULL, wed BIT NOT NULL, thu BIT NOT NULL, fri BIT NOT NULL, sat BIT NOT NULL, sun BIT NOT NULL, start_time INT NOT NULL, end_time INT NOT NULL, usage FLOAT NOT NULL 
		PRIMARY KEY CLUSTERED(legacy_call_letters ASC, mon ASC, tue ASC, wed ASC, thu ASC, fri ASC, sat ASC, sun ASC, start_time ASC, end_time ASC));
	INSERT INTO @share_usages
			SELECT
				rr.legacy_call_letters,rr.mon,rr.tue,rr.wed,rr.thu,rr.fri,rr.sat,rr.sun,u.start_time,u.end_time,
				SUM(CASE rr.mon WHEN 1 THEN u.weekday_usage ELSE 0 END +
				    CASE rr.tue WHEN 1 THEN u.weekday_usage ELSE 0 END +
				    CASE rr.wed WHEN 1 THEN u.weekday_usage ELSE 0 END +
				    CASE rr.thu WHEN 1 THEN u.weekday_usage ELSE 0 END +
				    CASE rr.fri WHEN 1 THEN u.weekday_usage ELSE 0 END +
				    CASE rr.sat WHEN 1 THEN u.weekend_usage ELSE 0 END +
				    CASE rr.sun WHEN 1 THEN u.weekend_usage ELSE 0 END) 'usage'
			FROM
				@ratings_request rr
				JOIN @share_market_codes hdi ON hdi.legacy_call_letters=rr.legacy_call_letters
				JOIN @share_min_playback_types mpt ON mpt.market_code=hdi.market_code
				CROSS APPLY @audience_ids a
				JOIN nsi.usages u (NOLOCK) ON u.media_month_id=@share_media_month_id
					AND u.playback_type=mpt.available_playback_type
					AND u.audience_id=a.audience_id
					AND (u.start_time<=rr.end_time AND u.end_time>=rr.start_time)
					AND u.market_code=hdi.market_code
				JOIN nsi.universes uv (NOLOCK) ON uv.media_month_id=u.media_month_id
					AND uv.playback_type=mpt.available_playback_type
					AND uv.market_code=u.market_code
					AND uv.audience_id=a.audience_id
		GROUP BY
			rr.legacy_call_letters,
			mon,tue,wed,thu,fri,sat,sun,
			u.start_time,u.end_time

	DECLARE @share_viewers TABLE (legacy_call_letters varchar(15) NOT NULL, mon BIT NOT NULL, tue BIT NOT NULL, wed BIT NOT NULL, thu BIT NOT NULL, fri BIT NOT NULL, sat BIT NOT NULL, sun BIT NOT NULL, start_time INT NOT NULL, end_time INT NOT NULL, viewers FLOAT NOT NULL 
		PRIMARY KEY CLUSTERED(legacy_call_letters ASC, mon ASC, tue ASC, wed ASC, thu ASC, fri ASC, sat ASC, sun ASC, start_time ASC, end_time ASC));
	INSERT INTO @share_viewers
			SELECT
				rr.legacy_call_letters,rr.mon,rr.tue,rr.wed,rr.thu,rr.fri,rr.sat,rr.sun,v.start_time,v.end_time,
				SUM(CASE rr.mon WHEN 1 THEN vd.weekday_viewers ELSE 0 END +
				    CASE rr.tue WHEN 1 THEN vd.weekday_viewers ELSE 0 END +
				    CASE rr.wed WHEN 1 THEN vd.weekday_viewers ELSE 0 END +
				    CASE rr.thu WHEN 1 THEN vd.weekday_viewers ELSE 0 END +
				    CASE rr.fri WHEN 1 THEN vd.weekday_viewers ELSE 0 END +
				    CASE rr.sat WHEN 1 THEN vd.weekend_viewers ELSE 0 END +
				    CASE rr.sun WHEN 1 THEN vd.weekend_viewers ELSE 0 END) 'viewers'
			FROM
				@ratings_request rr
				JOIN nsi.viewers v (NOLOCK) ON v.media_month_id=@share_media_month_id
					AND v.legacy_call_letters=rr.legacy_call_letters
					AND (v.start_time<=rr.end_time AND v.end_time>=rr.start_time)
				JOIN @share_min_playback_types mpt ON mpt.market_code=v.market_code
				CROSS APPLY @audience_ids a
				JOIN nsi.viewer_details vd (NOLOCK) ON vd.media_month_id=v.media_month_id
					AND vd.viewer_id=v.id
					AND vd.audience_id=a.audience_id
					AND vd.playback_type=mpt.available_playback_type
		GROUP BY
			rr.legacy_call_letters,mon,tue,wed,thu,fri,sat,sun,v.start_time,v.end_time

	DECLARE @share TABLE (legacy_call_letters varchar(15) NOT NULL, mon BIT NOT NULL, tue BIT NOT NULL, wed BIT NOT NULL, thu BIT NOT NULL, fri BIT NOT NULL, sat BIT NOT NULL, sun BIT NOT NULL, start_time INT NOT NULL, end_time INT NOT NULL, share FLOAT NOT NULL, 
		PRIMARY KEY CLUSTERED(legacy_call_letters ASC, mon ASC, tue ASC, wed ASC, thu ASC, fri ASC, sat ASC, sun ASC, start_time ASC, end_time ASC));
	INSERT INTO @share
		SELECT 
			v.legacy_call_letters,
			v.mon,
			v.tue,
			v.wed,
			v.thu,
			v.fri,
			v.sat,
			v.sun,
			v.start_time,
			v.end_time,
			CASE u.usage WHEN 0 THEN 0 ELSE v.viewers / u.usage END 'share'
		FROM 
			@share_viewers v
			JOIN @share_usages u ON u.legacy_call_letters = v.legacy_call_letters
				AND u.mon = v.mon
				AND u.tue = v.tue
				AND u.wed = v.wed
				AND u.thu = v.thu
				AND u.fri = v.fri
				AND u.sat = v.sat
				AND u.sun = v.sun
				AND u.start_time = v.start_time
				AND u.end_time = v.end_time;

	SELECT
		h.legacy_call_letters,h.mon,h.tue,h.wed,h.thu,h.fri,h.sat,h.sun,rr.start_time,rr.end_time,
		AVG(h.hut*s.share) 'rating'
	FROM
		@hut h
		JOIN @share s ON s.legacy_call_letters=h.legacy_call_letters
			AND s.mon=h.mon
			AND s.tue=h.tue
			AND s.wed=h.wed
			AND s.thu=h.thu
			AND s.fri=h.fri
			AND s.sat=h.sat
			AND s.sun=h.sun
			AND s.start_time=h.start_time
			AND s.end_time=h.end_time
		JOIN @ratings_request rr ON h.legacy_call_letters=rr.legacy_call_letters
			AND h.mon=rr.mon
			AND h.tue=rr.tue
			AND h.wed=rr.wed
			AND h.thu=rr.thu
			AND h.fri=rr.fri
			AND h.sat=rr.sat
			AND h.sun=rr.sun
			AND (h.start_time<=rr.end_time AND h.end_time>=rr.start_time)			
	GROUP BY
		h.legacy_call_letters,h.mon,h.tue,h.wed,h.thu,h.fri,h.sat,h.sun,rr.start_time,rr.end_time
END


GO


IF object_id('[nsi].[usp_ForecastNsiRatingsMonth]') IS NOT NULL
BEGIN
	DROP PROC [nsi].[usp_ForecastNsiRatingsMonth]
END

GO
CREATE PROCEDURE [nsi].[usp_ForecastNsiRatingsMonth]
       @media_month_id SMALLINT
AS
BEGIN
       SET NOCOUNT ON;
       SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
    SET DATEFIRST 1; -- MON

       DECLARE @sample_type VARCHAR(1) = '1'
       DECLARE @reporting_service VARCHAR(1) = '1'
       DECLARE @distribution_source_type VARCHAR(1) = 'B'
       DECLARE @station_type_code_to_exclude TINYINT = 2
       DECLARE @partition_number INT
       DECLARE @start_date DATE,
                     @end_date DATE

       SELECT
              @start_date = MIN(start_date_of_survey), 
              @end_date = MAX(end_date_of_survey) 
       FROM 
              nsi.market_headers 
       WHERE
              media_month_id = @media_month_id

       DECLARE @days_in_month TABLE (calendar_date DATE, date_part TINYINT)
       WHILE @start_date <= @end_date
       BEGIN
              INSERT INTO @days_in_month SELECT @start_date, DATEPART(weekday, @start_date)
              SET @start_date = DATEADD(day,1,@start_date);
       END

       DECLARE @half_hour_increments TABLE (start_time INT NOT NULL, end_time INT NOT NULL)
       DECLARE @current_half_hour INT = 0
       WHILE @current_half_hour <> 86400
       BEGIN
              INSERT INTO @half_hour_increments SELECT @current_half_hour,@current_half_hour+899
              SET @current_half_hour = @current_half_hour + 900
       END

       -- clear previous data if it exists
       SELECT TOP 1 @partition_number = $partition.[MediaMonthSmallIntPFN](media_month_id) FROM nsi.usages (NOLOCK) WHERE media_month_id=@media_month_id
       IF @partition_number IS NOT NULL
       BEGIN
              ALTER TABLE nsi.usages SWITCH PARTITION @partition_number TO nsi.usages_trunc PARTITION @partition_number
              TRUNCATE TABLE nsi.usages_trunc;
       END

       INSERT INTO nsi.usages
              SELECT
                     mh.media_month_id,
                     mh.playback_type,
                     qhea.audience_id,
                     hhi.start_time,
                     hhi.end_time,
                     mh.market_code,
                     ISNULL(AVG(CASE d.date_part WHEN 1 THEN qhea.usage ELSE NULL END), 0),
                     ISNULL(AVG(CASE d.date_part WHEN 2 THEN qhea.usage ELSE NULL END), 0),
                     ISNULL(AVG(CASE d.date_part WHEN 3 THEN qhea.usage ELSE NULL END), 0),
                     ISNULL(AVG(CASE d.date_part WHEN 4 THEN qhea.usage ELSE NULL END), 0),
                     ISNULL(AVG(CASE d.date_part WHEN 5 THEN qhea.usage ELSE NULL END), 0),
                     ISNULL(AVG(CASE d.date_part WHEN 6 THEN qhea.usage ELSE NULL END), 0),
                     ISNULL(AVG(CASE d.date_part WHEN 7 THEN qhea.usage ELSE NULL END), 0),
                     0,
                     0
              FROM
                     nsi.market_headers mh
                     JOIN nsi.quarter_hour_estimates qhe ON qhe.market_header_id=mh.id
                           AND qhe.sample_type=@sample_type
                     JOIN nsi.quarter_hour_estimate_audiences qhea ON qhea.quarter_hour_estimate_id=qhe.id
                     JOIN @days_in_month d ON d.calendar_date=qhe.calendar_date
                     JOIN @half_hour_increments hhi ON qhe.calendar_time BETWEEN hhi.start_time AND hhi.end_time
              WHERE
                     mh.media_month_id=@media_month_id
                     AND mh.reporting_service=@reporting_service
              GROUP BY
                     mh.media_month_id,
                     mh.playback_type,
                     qhea.audience_id,
                     hhi.start_time,
                     hhi.end_time,
                     mh.market_code


	CREATE TABLE #usages_averages (media_month_id SMALLINT NOT NULL, playback_type varchar(1) NOT NULL, audience_id INT NOT NULL, start_time int not null, end_time int not null, market_code smallint not null, week_part INT NOT NULL, usage FLOAT NOT NULL);
	INSERT INTO #usages_averages
              SELECT
                     mh.media_month_id,
                     mh.playback_type,
                     qhea.audience_id,
                     hhi.start_time,
                     hhi.end_time,
                     mh.market_code,
					CASE DATEPART(weekday, qhe.calendar_date)
						WHEN 1 THEN 0 WHEN 2 THEN 0 WHEN 3 THEN 0 WHEN 4 THEN 0 WHEN 5 THEN 0	-- weekday = 0
						WHEN 6 THEN 1 WHEN 7 THEN 1												-- weekend = 1
					END week_part,
					AVG(qhea.usage) usage
              FROM
                     nsi.market_headers mh
                     JOIN nsi.quarter_hour_estimates qhe ON qhe.market_header_id=mh.id
                           AND qhe.sample_type=@sample_type
                     JOIN nsi.quarter_hour_estimate_audiences qhea ON qhea.quarter_hour_estimate_id=qhe.id
                     JOIN @days_in_month d ON d.calendar_date=qhe.calendar_date
                     JOIN @half_hour_increments hhi ON qhe.calendar_time BETWEEN hhi.start_time AND hhi.end_time
              WHERE
                     mh.media_month_id=@media_month_id
                     AND mh.reporting_service=@reporting_service
              GROUP BY
                     mh.media_month_id,
                     mh.playback_type,
                     qhea.audience_id,
                     hhi.start_time,
                     hhi.end_time,
                     mh.market_code,
		     CASE DATEPART(weekday, qhe.calendar_date)
	      	     	WHEN 1 THEN 0 WHEN 2 THEN 0 WHEN 3 THEN 0 WHEN 4 THEN 0 WHEN 5 THEN 0	-- weekday = 0
   		        WHEN 6 THEN 1 WHEN 7 THEN 1												-- weekend = 1
		     END

		UPDATE 
			u
		SET 
			weekday_usage = usage
		FROM 
			nsi.usages u
		join 
			#usages_averages ua
		ON 
			u.media_month_id = ua.media_month_id
			and u.playback_type = ua.playback_type
			and u.audience_id = ua.audience_id
			and u.start_time = ua.start_time
			and u.end_time = ua.end_time
		WHERE 
			ua.week_part = 0;
		
		update 
			u
		SET 
			weekday_usage = usage
		FROM 
			nsi.usages u
		join 
			#usages_averages ua
		ON 
			u.media_month_id = ua.media_month_id
			and u.playback_type = ua.playback_type
			and u.audience_id = ua.audience_id
			and u.start_time = ua.start_time
			and u.end_time = ua.end_time
		WHERE 
			ua.week_part = 1;

       -- clear previous data if it exists
       SELECT TOP 1 @partition_number = $partition.[MediaMonthSmallIntPFN](media_month_id) FROM nsi.viewers (NOLOCK) WHERE media_month_id=@media_month_id
       IF @partition_number IS NOT NULL
       BEGIN
              ALTER TABLE nsi.viewers SWITCH PARTITION @partition_number TO nsi.viewers_trunc PARTITION @partition_number
              TRUNCATE TABLE nsi.viewers_trunc;
       END

       INSERT INTO nsi.viewers
		(media_month_id,legacy_call_letters,start_time,end_time,market_code)
              SELECT
                     mh.media_month_id,
                     dh.legacy_call_letters,
                     hhi.start_time,
                     hhi.end_time,
                     mh.market_code
              FROM
                     nsi.market_headers mh
                     JOIN nsi.distributor_headers dh ON dh.market_header_id=mh.id
                           AND dh.distribution_source_type=@distribution_source_type
                           AND dh.station_type_code <> @station_type_code_to_exclude
                     JOIN nsi.quarter_hour_distributor_estimates qhde ON qhde.market_header_id=mh.id
                           AND qhde.distributor_code=dh.distributor_code
                           AND qhde.sample_type=@sample_type
                     JOIN @half_hour_increments hhi ON qhde.calendar_time BETWEEN hhi.start_time AND hhi.end_time
              WHERE
                     mh.media_month_id=@media_month_id
                     AND mh.reporting_service=@reporting_service
              GROUP BY
                     mh.media_month_id,
                     dh.legacy_call_letters,
                     hhi.start_time,
                     hhi.end_time,
                     mh.market_code

       -- clear previous data if it exists
       SELECT TOP 1 @partition_number = $partition.[MediaMonthSmallIntPFN](media_month_id) FROM nsi.viewer_details (NOLOCK) WHERE media_month_id=@media_month_id
       IF @partition_number IS NOT NULL
       BEGIN
              ALTER TABLE nsi.viewer_details SWITCH PARTITION @partition_number TO nsi.viewer_details_trunc PARTITION @partition_number
              TRUNCATE TABLE nsi.viewer_details_trunc;
       END

       INSERT INTO nsi.viewer_details
              SELECT
                     v.media_month_id,
                     v.id,
                     qhdea.audience_id,
                     mh.playback_type,
                     ISNULL(AVG(CASE d.date_part WHEN 1 THEN qhdea.usage ELSE NULL END), 0),
                     ISNULL(AVG(CASE d.date_part WHEN 2 THEN qhdea.usage ELSE NULL END), 0),
                     ISNULL(AVG(CASE d.date_part WHEN 3 THEN qhdea.usage ELSE NULL END), 0),
                     ISNULL(AVG(CASE d.date_part WHEN 4 THEN qhdea.usage ELSE NULL END), 0),
                     ISNULL(AVG(CASE d.date_part WHEN 5 THEN qhdea.usage ELSE NULL END), 0),
                     ISNULL(AVG(CASE d.date_part WHEN 6 THEN qhdea.usage ELSE NULL END), 0),
                     ISNULL(AVG(CASE d.date_part WHEN 7 THEN qhdea.usage ELSE NULL END), 0),
                     0,
                     0
              FROM
                     nsi.viewers v
                     JOIN nsi.market_headers mh ON mh.media_month_id=v.media_month_id
                           AND mh.market_code=v.market_code
                           AND mh.reporting_service=@reporting_service
                     JOIN nsi.distributor_headers dh ON dh.market_header_id=mh.id
                           AND dh.legacy_call_letters=v.legacy_call_letters
                           AND dh.distribution_source_type=@distribution_source_type
                           AND dh.station_type_code <> @station_type_code_to_exclude
                     JOIN nsi.quarter_hour_distributor_estimates qhde ON qhde.market_header_id=mh.id
                           AND qhde.distributor_code=dh.distributor_code
                           AND qhde.sample_type=@sample_type
                           AND qhde.calendar_time BETWEEN v.start_time AND v.end_time
                     JOIN @days_in_month d ON d.calendar_date=qhde.calendar_date
                     JOIN nsi.quarter_hour_distributor_estimate_audiences qhdea ON qhdea.market_header_id=mh.id
                           AND qhde.id=qhdea.quarter_hour_distributor_estimate_id
              WHERE
                     v.media_month_id=@media_month_id
              GROUP BY
                     v.media_month_id,
                     v.id,
                     qhdea.audience_id,
                     mh.playback_type

	CREATE TABLE #viewer_details_averages (media_month_id SMALLINT NOT NULL, viewer_id bigint NOT NULL, audience_id INT NOT NULL, playback_type VARCHAR(1) NOT NULL, week_part INT NOT NULL, viewers FLOAT NOT NULL);
	INSERT INTO #viewer_details_averages
	SELECT
		v.media_month_id,
		v.id,
		qhdea.audience_id,
		mh.playback_type,
		CASE DATEPART(weekday, qhde.calendar_date)
			WHEN 1 THEN 0 WHEN 2 THEN 0 WHEN 3 THEN 0 WHEN 4 THEN 0 WHEN 5 THEN 0	-- weekday = 0
			WHEN 6 THEN 1 WHEN 7 THEN 1												-- weekend = 1
		END week_part,
		AVG(qhdea.usage) viewers
	FROM
		nsi.viewers v
		JOIN nsi.market_headers mh ON mh.media_month_id=v.media_month_id
			AND mh.market_code=v.market_code
			AND mh.reporting_service=@reporting_service
		JOIN nsi.distributor_headers dh ON dh.market_header_id=mh.id
			AND dh.legacy_call_letters=v.legacy_call_letters
			AND dh.distribution_source_type=@distribution_source_type
			AND dh.station_type_code <> @station_type_code_to_exclude
		JOIN nsi.quarter_hour_distributor_estimates qhde ON qhde.market_header_id=mh.id
			AND qhde.distributor_code=dh.distributor_code
			AND qhde.sample_type=@sample_type
			AND qhde.calendar_time BETWEEN v.start_time AND v.end_time
		JOIN @days_in_month d ON d.calendar_date=qhde.calendar_date
		JOIN nsi.quarter_hour_distributor_estimate_audiences qhdea ON qhdea.market_header_id=mh.id
			AND qhde.id=qhdea.quarter_hour_distributor_estimate_id
	WHERE
		v.media_month_id=@media_month_id
	GROUP BY
		v.media_month_id,
		v.id,
		qhdea.audience_id,
		mh.playback_type,
			CASE DATEPART(weekday, qhde.calendar_date)
				WHEN 1 THEN 0 WHEN 2 THEN 0 WHEN 3 THEN 0 WHEN 4 THEN 0 WHEN 5 THEN 0	-- weekday = 0
				WHEN 6 THEN 1 WHEN 7 THEN 1												-- weekend = 1
			END

		update 
			vd
		SET 
			weekday_viewers = viewers
		FROM 
			nsi.viewer_details vd 
		join 
			#viewer_details_averages vda 
		ON 
			vda.media_month_id = vd.media_month_id
			and vda.audience_id = vd.audience_id
			and vda.viewer_id = vd.viewer_id
			and vda.playback_type = vd.playback_type
		WHERE 
			vda.week_part = 0;
		
		
		update 
			vd
		SET 
			weekend_viewers = viewers
		FROM 
			nsi.viewer_details vd 
		join 
			#viewer_details_averages vda 
		ON 
			vda.media_month_id = vd.media_month_id
			and vda.audience_id = vd.audience_id
			and vda.viewer_id = vd.viewer_id
			and vda.playback_type = vd.playback_type
		WHERE 
			vda.week_part = 1;

		DROP TABLE #viewer_details_averages;
		DROP TABLE #usages_averages;

       -- clear previous data if it exists
       DELETE FROM nsi.universes WHERE media_month_id=@media_month_id;

       INSERT INTO nsi.universes
              SELECT
                     mh.media_month_id,
                     mh.playback_type,
                     mh.market_code,
                     mueha.audience_id,
                     mueha.universe
              FROM
                     nsi.market_headers mh
                     JOIN nsi.market_universe_estimate_headers mueh ON mueh.market_header_id=mh.id
                     JOIN nsi.market_universe_estimate_header_audiences mueha ON mueha.market_universe_estimate_header_id=mueh.id
              WHERE
                     mh.media_month_id=@media_month_id
                     AND mueh.sample_type=@sample_type
                     AND mh.reporting_service=@reporting_service


END
GO
IF object_id('[nsi].[usp_GetImpressionsForMultiplePrograms_Daypart]') IS NOT NULL
BEGIN
	DROP PROC [nsi].[usp_GetImpressionsForMultiplePrograms_Daypart]
END

GO


---- =============================================
---- Author:		
---- Create date: 05/18/2017
---- Description:	
---- =============================================
/*
	DECLARE
		@posting_media_month_id SMALLINT = 410,
		@demo VARCHAR(MAX) = '13,14,15,347,348',
		@ratings_request RatingsInputWithId,
		@min_playback_type VARCHAR(1) = '3'

	INSERT INTO @ratings_request SELECT 1 ,'legacy_call_letters',1,1,1,1,1,0,0,77400,79199
	INSERT INTO @ratings_request SELECT 2 ,'legacy_call_letters',1,1,1,1,1,0,0,25200,32399
    INSERT INTO @ratings_request SELECT 4 ,'legacy_call_letters',1,1,1,1,1,1,1,25200,79199
    INSERT INTO @ratings_request SELECT 5 ,'legacy_call_letters',1,1,1,1,1,1,1,25200,79199
    INSERT INTO @ratings_request SELECT 6 ,'legacy_call_letters',1,1,1,1,1,1,1,25200,79199
    INSERT INTO @ratings_request SELECT 7 ,'legacy_call_letters',1,1,1,1,1,1,1,25200,79199
    INSERT INTO @ratings_request SELECT 8 ,'legacy_call_letters',1,1,1,1,1,1,1,25200,79199
    INSERT INTO @ratings_request SELECT 9 ,'legacy_call_letters',1,1,1,1,1,1,1,25200,79199
    INSERT INTO @ratings_request SELECT 10,'legacy_call_letters',1,1,1,1,1,1,1,25200,79199
    INSERT INTO @ratings_request SELECT 11,'legacy_call_letters',1,1,1,1,1,1,1,25200,79199
    INSERT INTO @ratings_request SELECT 12,'legacy_call_letters',1,1,1,1,1,1,1,25200,79199
    INSERT INTO @ratings_request SELECT 13,'legacy_call_letters',1,1,1,1,1,1,1,25200,79199
    INSERT INTO @ratings_request SELECT 14,'legacy_call_letters',1,1,1,1,1,1,1,25200,79199
    INSERT INTO @ratings_request SELECT 15,'legacy_call_letters',1,1,1,1,1,1,1,25200,79199
    INSERT INTO @ratings_request SELECT 16,'legacy_call_letters',1,1,1,1,1,1,1,25200,79199
    INSERT INTO @ratings_request SELECT 17,'legacy_call_letters',1,1,1,1,1,1,1,25200,79199
    INSERT INTO @ratings_request SELECT 18,'legacy_call_letters',1,1,1,1,1,1,1,25200,79199
    INSERT INTO @ratings_request SELECT 19,'legacy_call_letters',1,1,1,1,1,1,1,25200,79199
	
	EXEC [nsi].[usp_GetImpressionsForMultiplePrograms_Daypart] @posting_media_month_id, @demo, @ratings_request, @min_playback_type
*/
--=============================================
CREATE PROCEDURE [nsi].[usp_GetImpressionsForMultiplePrograms_Daypart]
	@posting_media_month_id SMALLINT,
	@demo VARCHAR(MAX),
	@ratings_request RatingsInputWithId READONLY,
	@min_playback_type VARCHAR(1)
AS
BEGIN
	SET NOCOUNT ON;
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

	CREATE TABLE #rating_requests ([id] [int] NOT NULL,	[legacy_call_letters] varchar(15) NOT NULL,	[mon] [bit] NOT NULL,	[tue] [bit] NOT NULL,	[wed] [bit] NOT NULL,	[thu] [bit] NOT NULL,	[fri] [bit] NOT NULL,	[sat] [bit] NOT NULL,	[sun] [bit] NOT NULL,	[start_time] [int] NOT NULL,	[end_time] [int] NOT NULL,
		PRIMARY KEY CLUSTERED (	[id],	[legacy_call_letters],	[mon],	[tue],	[wed],	[thu],	[fri],	[sat],	[sun],	[start_time],	[end_time]))
	INSERT into #rating_requests 
		select * from @ratings_request 

	CREATE TABLE #audience_ids (audience_id INT NOT NULL, 
		PRIMARY KEY CLUSTERED(audience_id));
	INSERT INTO #audience_ids
		SELECT id FROM dbo.SplitIntegers(@demo);

	CREATE TABLE #min_playback_types (market_code SMALLINT NOT NULL, available_playback_type VARCHAR(1), 
		PRIMARY KEY CLUSTERED(market_code))
	INSERT INTO #min_playback_types
		SELECT * FROM nsi.udf_GetMinPlaybackTypes(@posting_media_month_id,@min_playback_type);

	CREATE TABLE #viewers (id INT NOT NULL, [legacy_call_letters] varchar(15) NOT NULL, audience_id INT NOT NULL, market_code SMALLINT NOT NULL, start_time INT NOT NULL, end_time INT NOT NULL, mon_viewers FLOAT NOT NULL, tue_viewers FLOAT NOT NULL, wed_viewers FLOAT NOT NULL, thu_viewers FLOAT NOT NULL, fri_viewers FLOAT NOT NULL, sat_viewers FLOAT NOT NULL, sun_viewers FLOAT NOT NULL, 
		PRIMARY KEY CLUSTERED(id, legacy_call_letters, audience_id, market_code, start_time, end_time))
	INSERT INTO #viewers
		SELECT
			rr.id, rr.legacy_call_letters,a.audience_id,v.market_code,v.start_time, v.end_time,
			CASE rr.mon WHEN 1 THEN vd.mon_viewers ELSE 0 END 'mon_viewers',
			CASE rr.tue WHEN 1 THEN vd.tue_viewers ELSE 0 END 'tue_viewers',
			CASE rr.wed WHEN 1 THEN vd.wed_viewers ELSE 0 END 'wed_viewers',
			CASE rr.thu WHEN 1 THEN vd.thu_viewers ELSE 0 END 'thu_viewers',
			CASE rr.fri WHEN 1 THEN vd.fri_viewers ELSE 0 END 'fri_viewers',
			CASE rr.sat WHEN 1 THEN vd.sat_viewers ELSE 0 END 'sat_viewers',
			CASE rr.sun WHEN 1 THEN vd.sun_viewers ELSE 0 END 'sun_viewers'
		FROM
			#rating_requests rr
			JOIN nsi.viewers v (NOLOCK) ON v.media_month_id=@posting_media_month_id
				AND v.legacy_call_letters=rr.legacy_call_letters
				AND (v.start_time<=rr.end_time AND v.end_time>=rr.start_time)
			JOIN #min_playback_types mpt ON mpt.market_code=v.market_code
			CROSS APPLY #audience_ids a
			JOIN nsi.viewer_details vd (NOLOCK) ON vd.media_month_id=v.media_month_id
				AND vd.viewer_id=v.id
				AND vd.audience_id=a.audience_id
				AND vd.playback_type=mpt.available_playback_type

	SELECT
		id,legacy_call_letters,audience_id,
		SUM(mon_viewers + tue_viewers + wed_viewers + thu_viewers + fri_viewers + sat_viewers + sun_viewers) 'impressions'
	FROM #viewers
	GROUP BY
		id,legacy_call_letters,audience_id

	IF OBJECT_ID('tempdb..#rating_requests') IS NOT NULL DROP TABLE #rating_requests;
	IF OBJECT_ID('tempdb..#min_playback_types') IS NOT NULL DROP TABLE #min_playback_types;
	IF OBJECT_ID('tempdb..#audience_ids') IS NOT NULL DROP TABLE #audience_ids;
	IF OBJECT_ID('tempdb..#viewers') IS NOT NULL DROP TABLE #viewers;
END

GO





IF object_id('[nsi].[usp_GetImpressionsForMultiplePrograms_Daypart_Averages]') IS NOT NULL
BEGIN
	DROP PROC [nsi].[usp_GetImpressionsForMultiplePrograms_Daypart_Averages]
END

GO

---- =============================================
---- Author:		
---- Create date: 08/21/2017
---- Description:	
---- =============================================
/*
	DECLARE
		@posting_media_month_id SMALLINT = 410,
		@demo VARCHAR(MAX) = '13,14,15,347,348',
		@ratings_request RatingsInputWithId,
		@min_playback_type VARCHAR(1) = '3'

	INSERT INTO @ratings_request SELECT 1,'legacy_call_letters',1,1,1,1,1,0,0,77400,79199
	INSERT INTO @ratings_request SELECT 2,'legacy_call_letters',1,1,1,1,1,0,0,25200,32399
    INSERT INTO @ratings_request SELECT 4,'legacy_call_letters',1,1,1,1,1,1,1,25200,79199
    INSERT INTO @ratings_request SELECT 5,'legacy_call_letters',1,1,1,1,1,1,1,25200,79199
    INSERT INTO @ratings_request SELECT 6,'legacy_call_letters',1,1,1,1,1,1,1,25200,79199
    INSERT INTO @ratings_request SELECT 7,'legacy_call_letters',1,1,1,1,1,1,1,25200,79199
    INSERT INTO @ratings_request SELECT 8,'legacy_call_letters',1,1,1,1,1,1,1,25200,79199
    INSERT INTO @ratings_request SELECT 9,'legacy_call_letters',1,1,1,1,1,1,1,25200,79199
    INSERT INTO @ratings_request SELECT 10,'legacy_call_letters',1,1,1,1,1,1,1,25200,79199
    INSERT INTO @ratings_request SELECT 11,'legacy_call_letters',1,1,1,1,1,1,1,25200,79199
    INSERT INTO @ratings_request SELECT 12,'legacy_call_letters',1,1,1,1,1,1,1,25200,79199
    INSERT INTO @ratings_request SELECT 13,'legacy_call_letters',1,1,1,1,1,1,1,25200,79199
    INSERT INTO @ratings_request SELECT 14,'legacy_call_letters',1,1,1,1,1,1,1,25200,79199
    INSERT INTO @ratings_request SELECT 15,'legacy_call_letters',1,1,1,1,1,1,1,25200,79199
    INSERT INTO @ratings_request SELECT 16,'legacy_call_letters',1,1,1,1,1,1,1,25200,79199
    INSERT INTO @ratings_request SELECT 17,'legacy_call_letters',1,1,1,1,1,1,1,25200,79199
    INSERT INTO @ratings_request SELECT 18,'legacy_call_letters',1,1,1,1,1,1,1,25200,79199
    INSERT INTO @ratings_request SELECT 19,'legacy_call_letters',1,1,1,1,1,1,1,25200,79199
	
	EXEC [nsi].[usp_GetImpressionsForMultiplePrograms_Daypart_Averages] @posting_media_month_id, @demo, @ratings_request, @min_playback_type
*/
--=============================================
CREATE PROCEDURE [nsi].[usp_GetImpressionsForMultiplePrograms_Daypart_Averages]
	@posting_media_month_id SMALLINT,
	@demo VARCHAR(MAX),
	@ratings_request RatingsInputWithId READONLY,
	@min_playback_type VARCHAR(1)
AS
BEGIN
	SET NOCOUNT ON;
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

	CREATE TABLE #rating_requests ([id] [int] NOT NULL,	[legacy_call_letters] varchar(15) NOT NULL,	[mon] [bit] NOT NULL,	[tue] [bit] NOT NULL,	[wed] [bit] NOT NULL,	[thu] [bit] NOT NULL,	[fri] [bit] NOT NULL,	[sat] [bit] NOT NULL,	[sun] [bit] NOT NULL,	[start_time] [int] NOT NULL,	[end_time] [int] NOT NULL,
		PRIMARY KEY CLUSTERED (	[id],	[legacy_call_letters],	[mon],	[tue],	[wed],	[thu],	[fri],	[sat],	[sun],	[start_time],	[end_time]))
	INSERT into #rating_requests 
		select * from @ratings_request 

	CREATE TABLE #audience_ids (audience_id INT NOT NULL, 
		PRIMARY KEY CLUSTERED(audience_id));
	INSERT INTO #audience_ids
		SELECT id FROM dbo.SplitIntegers(@demo);

	CREATE TABLE #min_playback_types (market_code SMALLINT NOT NULL, available_playback_type VARCHAR(1), PRIMARY KEY CLUSTERED(market_code))
	INSERT INTO #min_playback_types
		SELECT * FROM nsi.udf_GetMinPlaybackTypes(@posting_media_month_id,@min_playback_type);

	CREATE TABLE #viewers (id INT NOT NULL, legacy_call_letters varchar(15) NOT NULL, audience_id INT NOT NULL, market_code SMALLINT NOT NULL, start_time INT NOT NULL, end_time INT NOT NULL, viewers FLOAT NOT NULL, 
		PRIMARY KEY CLUSTERED(id, legacy_call_letters, audience_id, market_code, start_time, end_time))
	INSERT INTO #viewers
		SELECT
			rr.id, rr.legacy_call_letters,a.audience_id,v.market_code,v.start_time, v.end_time,
			CASE rr.mon WHEN 1 THEN vd.weekday_viewers ELSE 0 END +
			CASE rr.tue WHEN 1 THEN vd.weekday_viewers ELSE 0 END +
			CASE rr.wed WHEN 1 THEN vd.weekday_viewers ELSE 0 END +
			CASE rr.thu WHEN 1 THEN vd.weekday_viewers ELSE 0 END +
			CASE rr.fri WHEN 1 THEN vd.weekday_viewers ELSE 0 END +
			CASE rr.sat WHEN 1 THEN vd.weekend_viewers ELSE 0 END +
			CASE rr.sun WHEN 1 THEN vd.weekend_viewers ELSE 0 END
		FROM
			#rating_requests rr
			JOIN nsi.viewers v (NOLOCK) ON v.media_month_id=@posting_media_month_id
				AND v.legacy_call_letters=rr.legacy_call_letters
				AND (v.start_time<=rr.end_time AND v.end_time>=rr.start_time)
			JOIN #min_playback_types mpt ON mpt.market_code=v.market_code
			CROSS APPLY #audience_ids a
			JOIN nsi.viewer_details vd (NOLOCK) ON vd.media_month_id=v.media_month_id
				AND vd.viewer_id=v.id
				AND vd.audience_id=a.audience_id
				AND vd.playback_type=mpt.available_playback_type

	SELECT
		id,legacy_call_letters,audience_id,
		SUM(viewers) 'impressions'
	FROM #viewers
	GROUP BY
		id,legacy_call_letters,audience_id

	IF OBJECT_ID('tempdb..#rating_requests') IS NOT NULL DROP TABLE #rating_requests;
	IF OBJECT_ID('tempdb..#min_playback_types') IS NOT NULL DROP TABLE #min_playback_types;
	IF OBJECT_ID('tempdb..#audience_ids') IS NOT NULL DROP TABLE #audience_ids;
	IF OBJECT_ID('tempdb..#viewers') IS NOT NULL DROP TABLE #viewers;
END

GO
IF object_id('[nsi].[usp_GetImpressionsForMultiplePrograms_TwoBooks]') IS NOT NULL
BEGIN
	DROP PROC [nsi].[usp_GetImpressionsForMultiplePrograms_TwoBooks]
END

GO

-- =============================================
-- Author:		
-- Create date: 6/28/2017
-- Description:	
-- =============================================
/*
	DECLARE
		@hut_media_month_id SMALLINT = 416,
		@share_media_month_id SMALLINT = 422,
		@demo VARCHAR(MAX) = '6,7,21,22,284,348',
		@ratings_request RatingsInputWithId,
		@min_playback_type VARCHAR(1) = '3'

	INSERT INTO @ratings_request SELECT 1,5245,1,1,1,1,1,0,0,0,79199
	INSERT INTO @ratings_request SELECT 2,5245,1,1,1,1,1,0,0,0,32399
    INSERT INTO @ratings_request SELECT 4,1272,1,1,1,1,1,1,1,0,79199
    INSERT INTO @ratings_request SELECT 5,1292,1,1,1,1,1,1,1,12345,79199
    INSERT INTO @ratings_request SELECT 6,1297,1,1,1,1,1,1,1,0,79199
    INSERT INTO @ratings_request SELECT 7,1307,1,1,1,1,1,1,1,0,79199
    INSERT INTO @ratings_request SELECT 8,1314,1,1,1,1,1,1,1,34634,79199
    INSERT INTO @ratings_request SELECT 9,1316,1,1,1,1,1,1,1,0,79199
    INSERT INTO @ratings_request SELECT 10,1336,1,1,1,1,1,1,1,0,79199
    INSERT INTO @ratings_request SELECT 11,1351,1,1,1,1,1,1,1,0,79199
    INSERT INTO @ratings_request SELECT 12,1372,1,1,1,1,1,1,1,0,79199
    INSERT INTO @ratings_request SELECT 13,1378,1,1,1,1,1,1,1,0,79199
    INSERT INTO @ratings_request SELECT 14,1379,1,1,1,1,1,1,1,0,79199
    INSERT INTO @ratings_request SELECT 15,1387,1,1,1,1,1,1,1,0,79199
    INSERT INTO @ratings_request SELECT 16,1388,1,1,1,1,1,1,1,0,79199
    INSERT INTO @ratings_request SELECT 17,1423,1,1,1,1,1,1,1,0,79199
    INSERT INTO @ratings_request SELECT 18,1427,1,1,1,1,1,1,1,0,79199
    INSERT INTO @ratings_request SELECT 19,1459,1,1,1,1,1,1,1,0,79199

	EXEC [nsi].[usp_GetImpressionsForMultiplePrograms_TwoBooks] @hut_media_month_id, @share_media_month_id, @demo, @ratings_request, @min_playback_type
*/
 --=============================================
CREATE PROCEDURE [nsi].[usp_GetImpressionsForMultiplePrograms_TwoBooks]
	@hut_media_month_id SMALLINT,
	@share_media_month_id SMALLINT,
	@demo VARCHAR(MAX),
	@ratings_request RatingsInputWithId READONLY,
	@min_playback_type VARCHAR(1)
AS
BEGIN
	SET NOCOUNT ON;
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

	CREATE TABLE #rating_requests ([id] [int] NOT NULL,	[legacy_call_letters] varchar(15) NOT NULL,	[mon] [bit] NOT NULL,	[tue] [bit] NOT NULL,	[wed] [bit] NOT NULL,	[thu] [bit] NOT NULL,	[fri] [bit] NOT NULL,	[sat] [bit] NOT NULL,	[sun] [bit] NOT NULL,	[start_time] [int] NOT NULL,	[end_time] [int] NOT NULL,
		PRIMARY KEY CLUSTERED (	[id],	[legacy_call_letters],	[mon],	[tue],	[wed],	[thu],	[fri],	[sat],	[sun],	[start_time],	[end_time]))
	INSERT into #rating_requests 
		select * from @ratings_request 

	CREATE TABLE #audience_ids (audience_id INT NOT NULL, 
		PRIMARY KEY CLUSTERED(audience_id));
	INSERT INTO #audience_ids
		SELECT DISTINCT id FROM dbo.SplitIntegers(@demo);

	CREATE TABLE #hut_market_codes (id INT NOT NULL, market_code SMALLINT NOT NULL, min_playback_type VARCHAR(1), 
		PRIMARY KEY CLUSTERED(id, market_code, min_playback_type))
	INSERT INTO #hut_market_codes
		SELECT DISTINCT
			rr.id,
			v.market_code,
			mpt.available_playback_type
		FROM
			#rating_requests rr
			JOIN nsi.viewers v (NOLOCK) ON @hut_media_month_id = v.media_month_id
				AND v.legacy_call_letters=rr.legacy_call_letters
				AND (v.start_time<=rr.end_time AND v.end_time>=rr.start_time)
			JOIN (
			SELECT * FROM nsi.udf_GetMinPlaybackTypes(@hut_media_month_id,@min_playback_type)
			) mpt ON mpt.market_code=v.market_code

	CREATE TABLE #usage_days (id INT NOT NULL, [legacy_call_letters] varchar(15) NOT NULL, start_time int NOT NULL, end_time int NOT NULL, market_code SMALLINT NOT NULL, audience_id INT NOT NULL, mon_usage FLOAT NOT NULL, tue_usage FLOAT NOT NULL, wed_usage FLOAT NOT NULL, thu_usage FLOAT NOT NULL, fri_usage FLOAT NOT NULL, sat_usage FLOAT NOT NULL, sun_usage FLOAT NOT NULL, PRIMARY KEY(id, legacy_call_letters, start_time, end_time, market_code, audience_id))
	INSERT INTO #usage_days
		SELECT
			rr.id,rr.legacy_call_letters,u.start_time,u.end_time,hmc.market_code,a.audience_id,
			CASE rr.mon WHEN 1 THEN u.mon_usage ELSE 0 END,
			CASE rr.tue WHEN 1 THEN u.tue_usage ELSE 0 END,
			CASE rr.wed WHEN 1 THEN u.wed_usage ELSE 0 END,
			CASE rr.thu WHEN 1 THEN u.thu_usage ELSE 0 END,
			CASE rr.fri WHEN 1 THEN u.fri_usage ELSE 0 END,
			CASE rr.sat WHEN 1 THEN u.sat_usage ELSE 0 END,
			CASE rr.sun WHEN 1 THEN u.sun_usage ELSE 0 END
		FROM
			#rating_requests rr
			JOIN #hut_market_codes hmc ON hmc.id=rr.id
			CROSS APPLY #audience_ids a
			JOIN nsi.usages u (NOLOCK) ON u.media_month_id=@hut_media_month_id
				AND u.playback_type=hmc.min_playback_type
				AND u.audience_id=a.audience_id
				AND (u.start_time<=rr.end_time AND u.end_time>=rr.start_time)
				AND u.market_code=hmc.market_code

	CREATE TABLE #usage (id INT NOT NULL, [legacy_call_letters] varchar(15) NOT NULL, start_time INT NOT NULL, end_time INT NOT NULL, usage FLOAT NOT NULL, 
		PRIMARY KEY CLUSTERED(id, legacy_call_letters, start_time, end_time));
	INSERT INTO #usage
		SELECT
			id,legacy_call_letters,u.start_time,u.end_time,
			SUM(u.mon_usage +
			    u.tue_usage +
			    u.wed_usage +
			    u.thu_usage +
			    u.fri_usage +
			    u.sat_usage +
			    u.sun_usage) 'usage'
		FROM
			#usage_days u
		GROUP BY
			id,legacy_call_letters,start_time,end_time

	CREATE TABLE #share_market_codes(id INT NOT NULL, market_code SMALLINT NOT NULL, available_playback_type VARCHAR(1) NOT NULL, 
		PRIMARY KEY CLUSTERED(id, market_code, available_playback_type))
	INSERT INTO #share_market_codes
		SELECT DISTINCT
			rr.id,
			v.market_code,
			mpt.available_playback_type
		FROM
			#rating_requests rr
			JOIN nsi.viewers v (NOLOCK) ON @share_media_month_id = v.media_month_id
				AND v.legacy_call_letters=rr.legacy_call_letters
				AND (v.start_time<=rr.end_time AND v.end_time>=rr.start_time)
			JOIN (
			SELECT * FROM nsi.udf_GetMinPlaybackTypes(@share_media_month_id,@min_playback_type)
			) mpt on mpt.market_code = v.market_code;

	CREATE TABLE #share_usage_days (id INT NOT NULL, [legacy_call_letters] varchar(15) NOT NULL, start_time int NOT NULL, end_time int NOT NULL, market_code SMALLINT NOT NULL, audience_id INT NOT NULL, mon_usage FLOAT NOT NULL, tue_usage FLOAT NOT NULL, wed_usage FLOAT NOT NULL, thu_usage FLOAT NOT NULL, fri_usage FLOAT NOT NULL, sat_usage FLOAT NOT NULL, sun_usage FLOAT NOT NULL, 
		PRIMARY KEY(id, legacy_call_letters, start_time, end_time, market_code, audience_id))
	INSERT INTO #share_usage_days
		SELECT
			rr.id,rr.legacy_call_letters,u.start_time,u.end_time,mc.market_code,a.audience_id,
			CASE rr.mon WHEN 1 THEN u.mon_usage ELSE 0 END,
			CASE rr.tue WHEN 1 THEN u.tue_usage ELSE 0 END,
			CASE rr.wed WHEN 1 THEN u.wed_usage ELSE 0 END,
			CASE rr.thu WHEN 1 THEN u.thu_usage ELSE 0 END,
			CASE rr.fri WHEN 1 THEN u.fri_usage ELSE 0 END,
			CASE rr.sat WHEN 1 THEN u.sat_usage ELSE 0 END,
			CASE rr.sun WHEN 1 THEN u.sun_usage ELSE 0 END
		FROM
			#rating_requests rr
			JOIN #share_market_codes mc ON mc.id=rr.id
			CROSS APPLY #audience_ids a
			JOIN nsi.usages u (NOLOCK) ON u.media_month_id=@share_media_month_id
				AND u.playback_type=mc.available_playback_type
				AND u.audience_id=a.audience_id
				AND (u.start_time<=rr.end_time AND u.end_time>=rr.start_time)
				AND u.market_code=mc.market_code

	CREATE TABLE #share_usages (id INT NOT NULL, [legacy_call_letters] varchar(15) NOT NULL, start_time INT NOT NULL, end_time INT NOT NULL, usage FLOAT NOT NULL, 
		PRIMARY KEY CLUSTERED(id, legacy_call_letters, start_time, end_time));
	INSERT INTO #share_usages
		SELECT
			id,legacy_call_letters,u.start_time,u.end_time,
			SUM(u.mon_usage +
			    u.tue_usage +
			    u.wed_usage +
			    u.thu_usage +
			    u.fri_usage +
			    u.sat_usage +
			    u.sun_usage) 'usage'
		FROM
			#share_usage_days u
		GROUP BY
			id,legacy_call_letters,start_time,end_time

	CREATE TABLE #share_viewer_days (id INT NOT NULL, [legacy_call_letters] varchar(15) NOT NULL, start_time int NOT NULL, end_time int NOT NULL, market_code SMALLINT NOT NULL, audience_id INT NOT NULL, mon_viewers FLOAT NOT NULL, tue_viewers FLOAT NOT NULL, wed_viewers FLOAT NOT NULL, thu_viewers FLOAT NOT NULL, fri_viewers FLOAT NOT NULL, sat_viewers FLOAT NOT NULL, sun_viewers FLOAT NOT NULL, 
		PRIMARY KEY(id, legacy_call_letters, start_time, end_time, market_code, audience_id))
	INSERT INTO #share_viewer_days
		SELECT
			rr.id,rr.legacy_call_letters,v.start_time,v.end_time,mc.market_code,a.audience_id,
			CASE rr.mon WHEN 1 THEN vd.mon_viewers ELSE 0 END,
			CASE rr.tue WHEN 1 THEN vd.tue_viewers ELSE 0 END,
			CASE rr.wed WHEN 1 THEN vd.wed_viewers ELSE 0 END,
			CASE rr.thu WHEN 1 THEN vd.thu_viewers ELSE 0 END,
			CASE rr.fri WHEN 1 THEN vd.fri_viewers ELSE 0 END,
			CASE rr.sat WHEN 1 THEN vd.sat_viewers ELSE 0 END,
			CASE rr.sun WHEN 1 THEN vd.sun_viewers ELSE 0 END
		FROM
			#rating_requests rr
			JOIN #share_market_codes mc ON mc.id=rr.id
			JOIN nsi.viewers v (NOLOCK) ON v.media_month_id=@share_media_month_id
				AND v.legacy_call_letters=rr.legacy_call_letters
				AND (v.start_time<=rr.end_time AND v.end_time>=rr.start_time)
				AND v.market_code = mc.market_code
			CROSS APPLY #audience_ids a
			JOIN nsi.viewer_details vd (NOLOCK) ON 
				vd.media_month_id=v.media_month_id
				AND vd.viewer_id=v.id
				AND vd.audience_id=a.audience_id
				AND vd.playback_type=mc.available_playback_type

	CREATE TABLE #share_viewers (id INT NOT NULL, [legacy_call_letters] varchar(15) NOT NULL, start_time INT NOT NULL, end_time INT NOT NULL, viewers FLOAT NOT NULL, 
		PRIMARY KEY CLUSTERED(id, legacy_call_letters, start_time, end_time));
	INSERT INTO #share_viewers
		SELECT
			id,legacy_call_letters,start_time,end_time,
			SUM(mon_viewers +
				tue_viewers +
				wed_viewers +
				thu_viewers +
				fri_viewers +
				sat_viewers +
				sun_viewers )'viewers'
		FROM
			#share_viewer_days
		GROUP BY
			id,legacy_call_letters,start_time,end_time
	
	CREATE TABLE #share(id INT NOT NULL,[legacy_call_letters] varchar(15) NOT NULL, start_time INT NOT NULL, end_time INT NOT NULL, share FLOAT NOT NULL, 
		PRIMARY KEY CLUSTERED(id, legacy_call_letters, start_time, end_time));
	INSERT INTO #share
		SELECT 
			v.id,
			v.legacy_call_letters,
			v.start_time,
			v.end_time,
			CASE u.usage WHEN 0 THEN 0 ELSE v.viewers / u.usage END 'share'
		FROM 
			#share_viewers v
			JOIN #share_usages u ON 
				u.id = v.id
				AND u.legacy_call_letters = v.legacy_call_letters
				AND u.start_time = v.start_time
				AND u.end_time = v.end_time;

	SELECT
		h.id,h.legacy_call_letters,
		AVG(h.usage*s.share) 'impressions'
	FROM
		#usage h
		JOIN #share s ON 
			s.id = h.id
			AND s.legacy_call_letters=h.legacy_call_letters
			AND s.start_time=h.start_time
			AND s.end_time=h.end_time
		JOIN #rating_requests rr ON 
			h.id = rr.id 
			AND h.legacy_call_letters=rr.legacy_call_letters
	GROUP BY
		h.id, h.legacy_call_letters

	IF OBJECT_ID('tempdb..#rating_requests') IS NOT NULL DROP TABLE #rating_requests;
	IF OBJECT_ID('tempdb..#audience_ids') IS NOT NULL DROP TABLE #audience_ids;
	IF OBJECT_ID('tempdb..#hut_market_codes') IS NOT NULL DROP TABLE #hut_market_codes;
	IF OBJECT_ID('tempdb..#usage_days') IS NOT NULL DROP TABLE #usage_days;
	IF OBJECT_ID('tempdb..#usage') IS NOT NULL DROP TABLE #usage;
	IF OBJECT_ID('tempdb..#share_market_codes') IS NOT NULL DROP TABLE #share_market_codes;
	IF OBJECT_ID('tempdb..#share_usage_days') IS NOT NULL DROP TABLE #share_usage_days;
	IF OBJECT_ID('tempdb..#share_usages') IS NOT NULL DROP TABLE #share_usages;
	IF OBJECT_ID('tempdb..#share_viewer_days') IS NOT NULL DROP TABLE #share_viewer_days;
	IF OBJECT_ID('tempdb..#share_viewers') IS NOT NULL DROP TABLE #share_viewers;
	IF OBJECT_ID('tempdb..#share') IS NOT NULL DROP TABLE #share;
END

GO
IF object_id('[nsi].[usp_GetImpressionsForMultiplePrograms_TwoBooks_Averages]') IS NOT NULL
BEGIN
	DROP PROC [nsi].[usp_GetImpressionsForMultiplePrograms_TwoBooks_Averages]
END

GO



-- =============================================
-- Author:		
-- Create date: 8/21/2017
-- Description:	
-- =============================================
/*
	DECLARE
		@hut_media_month_id SMALLINT = 416,
		@share_media_month_id SMALLINT = 422,
		@demo VARCHAR(MAX) = '6,7,21,22,284,348',
		@ratings_request RatingsInputWithId,
		@min_playback_type VARCHAR(1) = '3'

	INSERT INTO @ratings_request SELECT 1,'legacy_call_letters',1,1,1,1,1,0,0,0,79199
	INSERT INTO @ratings_request SELECT 2,'legacy_call_letters',1,1,1,1,1,0,0,0,32399
    INSERT INTO @ratings_request SELECT 4,'legacy_call_letters',1,1,1,1,1,1,1,0,79199
    INSERT INTO @ratings_request SELECT 5,'legacy_call_letters',1,1,1,1,1,1,1,12345,79199
    INSERT INTO @ratings_request SELECT 6,'legacy_call_letters',1,1,1,1,1,1,1,0,79199
    INSERT INTO @ratings_request SELECT 7,'legacy_call_letters',1,1,1,1,1,1,1,0,79199
    INSERT INTO @ratings_request SELECT 8,'legacy_call_letters',1,1,1,1,1,1,1,34634,79199
    INSERT INTO @ratings_request SELECT 9,'legacy_call_letters',1,1,1,1,1,1,1,0,79199
    INSERT INTO @ratings_request SELECT 10,'legacy_call_letters',1,1,1,1,1,1,1,0,79199
    INSERT INTO @ratings_request SELECT 11,'legacy_call_letters',1,1,1,1,1,1,1,0,79199
    INSERT INTO @ratings_request SELECT 12,'legacy_call_letters',1,1,1,1,1,1,1,0,79199
    INSERT INTO @ratings_request SELECT 13,'legacy_call_letters',1,1,1,1,1,1,1,0,79199
    INSERT INTO @ratings_request SELECT 14,'legacy_call_letters',1,1,1,1,1,1,1,0,79199
    INSERT INTO @ratings_request SELECT 15,'legacy_call_letters',1,1,1,1,1,1,1,0,79199
    INSERT INTO @ratings_request SELECT 16,'legacy_call_letters',1,1,1,1,1,1,1,0,79199
    INSERT INTO @ratings_request SELECT 17,'legacy_call_letters',1,1,1,1,1,1,1,0,79199
    INSERT INTO @ratings_request SELECT 18,'legacy_call_letters',1,1,1,1,1,1,1,0,79199
    INSERT INTO @ratings_request SELECT 19,'legacy_call_letters',1,1,1,1,1,1,1,0,79199

	EXEC [nsi].[usp_GetImpressionsForMultiplePrograms_TwoBooks_Averages] @hut_media_month_id, @share_media_month_id, @demo, @ratings_request, @min_playback_type
*/
 --=============================================
CREATE PROCEDURE [nsi].[usp_GetImpressionsForMultiplePrograms_TwoBooks_Averages]
	@hut_media_month_id SMALLINT,
	@share_media_month_id SMALLINT,
	@demo VARCHAR(MAX),
	@ratings_request RatingsInputWithId READONLY,
	@min_playback_type VARCHAR(1)
AS
BEGIN
	SET NOCOUNT ON;
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

	CREATE TABLE #rating_requests ([id] [int] NOT NULL,	[legacy_call_letters] varchar(15) NOT NULL,	[mon] [bit] NOT NULL,	[tue] [bit] NOT NULL,	[wed] [bit] NOT NULL,	[thu] [bit] NOT NULL,	[fri] [bit] NOT NULL,	[sat] [bit] NOT NULL,	[sun] [bit] NOT NULL,	[start_time] [int] NOT NULL,	[end_time] [int] NOT NULL,
		PRIMARY KEY CLUSTERED (	[id],	[legacy_call_letters],	[mon],	[tue],	[wed],	[thu],	[fri],	[sat],	[sun],	[start_time],	[end_time]))
	INSERT into #rating_requests 
		select * from @ratings_request 

	CREATE TABLE #audience_ids (audience_id INT NOT NULL, 
		PRIMARY KEY CLUSTERED(audience_id));
	INSERT INTO #audience_ids
		SELECT DISTINCT id FROM dbo.SplitIntegers(@demo);

	CREATE TABLE #hut_market_codes (id INT NOT NULL, market_code SMALLINT NOT NULL, min_playback_type VARCHAR(1), 
		PRIMARY KEY CLUSTERED(id, market_code, min_playback_type))
	INSERT INTO #hut_market_codes
		SELECT DISTINCT
			rr.id,
			v.market_code,
			mpt.available_playback_type
		FROM
			#rating_requests rr
			JOIN nsi.viewers v (NOLOCK) ON @hut_media_month_id = v.media_month_id
				AND v.legacy_call_letters=rr.legacy_call_letters
				AND (v.start_time<=rr.end_time AND v.end_time>=rr.start_time)
			JOIN (
			SELECT * FROM nsi.udf_GetMinPlaybackTypes(@hut_media_month_id,@min_playback_type)
			) mpt ON mpt.market_code=v.market_code

	CREATE TABLE #usage_days (id INT NOT NULL, legacy_call_letters varchar(15) NOT NULL, start_time int NOT NULL, end_time int NOT NULL, market_code SMALLINT NOT NULL, audience_id INT NOT NULL, usage FLOAT NOT NULL, 
		PRIMARY KEY(id, legacy_call_letters, start_time, end_time, market_code, audience_id))
	INSERT INTO #usage_days
		SELECT
			rr.id,rr.legacy_call_letters,u.start_time,u.end_time,hmc.market_code,a.audience_id,
			CASE rr.mon WHEN 1 THEN u.weekday_usage ELSE 0 END+
			CASE rr.tue WHEN 1 THEN u.weekday_usage ELSE 0 END+
			CASE rr.wed WHEN 1 THEN u.weekday_usage ELSE 0 END+
			CASE rr.thu WHEN 1 THEN u.weekday_usage ELSE 0 END+
			CASE rr.fri WHEN 1 THEN u.weekday_usage ELSE 0 END+
			CASE rr.sat WHEN 1 THEN u.weekend_usage ELSE 0 END+
			CASE rr.sun WHEN 1 THEN u.weekend_usage ELSE 0 END
		FROM
			#rating_requests rr
			JOIN #hut_market_codes hmc ON hmc.id=rr.id
			CROSS APPLY #audience_ids a
			JOIN nsi.usages u (NOLOCK) ON u.media_month_id=@hut_media_month_id
				AND u.playback_type=hmc.min_playback_type
				AND u.audience_id=a.audience_id
				AND (u.start_time<=rr.end_time AND u.end_time>=rr.start_time)
				AND u.market_code=hmc.market_code

	CREATE TABLE #usage (id INT NOT NULL, [legacy_call_letters] varchar(15) NOT NULL, start_time INT NOT NULL, end_time INT NOT NULL, usage FLOAT NOT NULL, 
		PRIMARY KEY CLUSTERED(id, legacy_call_letters, start_time, end_time));
	INSERT INTO #usage
		SELECT
			id,legacy_call_letters,u.start_time,u.end_time,
			SUM(u.usage) 'usage'
		FROM
			#usage_days u
		GROUP BY
			id,legacy_call_letters,start_time,end_time

	CREATE TABLE #share_market_codes(id INT NOT NULL, market_code SMALLINT NOT NULL, available_playback_type VARCHAR(1) NOT NULL, 
		PRIMARY KEY CLUSTERED(id, market_code, available_playback_type))
	INSERT INTO #share_market_codes
		SELECT DISTINCT
			rr.id,
			v.market_code,
			mpt.available_playback_type
		FROM
			#rating_requests rr
			JOIN nsi.viewers v (NOLOCK) ON @share_media_month_id = v.media_month_id
				AND v.legacy_call_letters=rr.legacy_call_letters
				AND (v.start_time<=rr.end_time AND v.end_time>=rr.start_time)
			JOIN (
			SELECT * FROM nsi.udf_GetMinPlaybackTypes(@share_media_month_id,@min_playback_type)
			) mpt on mpt.market_code = v.market_code;

	CREATE TABLE #share_usage_days (id INT NOT NULL, [legacy_call_letters] varchar(15) NOT NULL, start_time int NOT NULL, end_time int NOT NULL, market_code SMALLINT NOT NULL, audience_id INT NOT NULL, usage FLOAT NOT NULL, 
		PRIMARY KEY(id, legacy_call_letters, start_time, end_time, market_code, audience_id))
	INSERT INTO #share_usage_days
		SELECT
			rr.id,rr.legacy_call_letters,u.start_time,u.end_time,mc.market_code,a.audience_id,
			CASE rr.mon WHEN 1 THEN u.weekday_usage ELSE 0 END+
			CASE rr.tue WHEN 1 THEN u.weekday_usage ELSE 0 END+
			CASE rr.wed WHEN 1 THEN u.weekday_usage ELSE 0 END+
			CASE rr.thu WHEN 1 THEN u.weekday_usage ELSE 0 END+
			CASE rr.fri WHEN 1 THEN u.weekday_usage ELSE 0 END+
			CASE rr.sat WHEN 1 THEN u.weekend_usage ELSE 0 END+
			CASE rr.sun WHEN 1 THEN u.weekend_usage ELSE 0 END
		FROM
			#rating_requests rr
			JOIN #share_market_codes mc ON mc.id=rr.id
			CROSS APPLY #audience_ids a
			JOIN nsi.usages u (NOLOCK) ON u.media_month_id=@share_media_month_id
				AND u.playback_type=mc.available_playback_type
				AND u.audience_id=a.audience_id
				AND (u.start_time<=rr.end_time AND u.end_time>=rr.start_time)
				AND u.market_code=mc.market_code

	CREATE TABLE #share_usages (id INT NOT NULL, [legacy_call_letters] varchar(15) NOT NULL, start_time INT NOT NULL, end_time INT NOT NULL, usage FLOAT NOT NULL, 
		PRIMARY KEY CLUSTERED(id, legacy_call_letters, start_time, end_time));
	INSERT INTO #share_usages
		SELECT
			id,legacy_call_letters,u.start_time,u.end_time,
			SUM(usage) 'usage'
		FROM
			#share_usage_days u
		GROUP BY
			id,legacy_call_letters,start_time,end_time

	CREATE TABLE #share_viewer_days (id INT NOT NULL, [legacy_call_letters] varchar(15) NOT NULL, start_time int NOT NULL, end_time int NOT NULL, market_code SMALLINT NOT NULL, audience_id INT NOT NULL, viewers FLOAT NOT NULL, 
		PRIMARY KEY(id, legacy_call_letters, start_time, end_time, market_code, audience_id))
	INSERT INTO #share_viewer_days
		SELECT
			rr.id,rr.legacy_call_letters,v.start_time,v.end_time,mc.market_code,a.audience_id,
			CASE rr.mon WHEN 1 THEN vd.weekday_viewers ELSE 0 END+
			CASE rr.tue WHEN 1 THEN vd.weekday_viewers ELSE 0 END+
			CASE rr.wed WHEN 1 THEN vd.weekday_viewers ELSE 0 END+
			CASE rr.thu WHEN 1 THEN vd.weekday_viewers ELSE 0 END+
			CASE rr.fri WHEN 1 THEN vd.weekday_viewers ELSE 0 END+
			CASE rr.sat WHEN 1 THEN vd.weekend_viewers ELSE 0 END+
			CASE rr.sun WHEN 1 THEN vd.weekend_viewers ELSE 0 END
		FROM
			#rating_requests rr
			JOIN #share_market_codes mc ON mc.id=rr.id
			JOIN nsi.viewers v (NOLOCK) ON v.media_month_id=@share_media_month_id
				AND v.legacy_call_letters=rr.legacy_call_letters
				AND (v.start_time<=rr.end_time AND v.end_time>=rr.start_time)
				AND v.market_code = mc.market_code
			CROSS APPLY #audience_ids a
			JOIN nsi.viewer_details vd (NOLOCK) ON 
				vd.media_month_id=v.media_month_id
				AND vd.viewer_id=v.id
				AND vd.audience_id=a.audience_id
				AND vd.playback_type=mc.available_playback_type

	CREATE TABLE #share_viewers (id INT NOT NULL, [legacy_call_letters] varchar(15) NOT NULL, start_time INT NOT NULL, end_time INT NOT NULL, viewers FLOAT NOT NULL, 
		PRIMARY KEY CLUSTERED(id, legacy_call_letters, start_time, end_time));
	INSERT INTO #share_viewers
		SELECT
			id,legacy_call_letters,start_time,end_time,
			SUM(viewers)'viewers'
		FROM
			#share_viewer_days
		GROUP BY
			id,legacy_call_letters,start_time,end_time
	
	CREATE TABLE #share(id INT NOT NULL,[legacy_call_letters] varchar(15) NOT NULL, start_time INT NOT NULL, end_time INT NOT NULL, share FLOAT NOT NULL, 
		PRIMARY KEY CLUSTERED(id, legacy_call_letters, start_time, end_time));
	INSERT INTO #share
		SELECT 
			v.id,
			v.legacy_call_letters,
			v.start_time,
			v.end_time,
			CASE u.usage WHEN 0 THEN 0 ELSE v.viewers / u.usage END 'share'
		FROM 
			#share_viewers v
			JOIN #share_usages u ON 
				u.id = v.id
				AND u.legacy_call_letters = v.legacy_call_letters
				AND u.start_time = v.start_time
				AND u.end_time = v.end_time;

	SELECT
		h.id,h.legacy_call_letters,
		AVG(h.usage*s.share) 'impressions'
	FROM
		#usage h
		JOIN #share s ON 
			s.id = h.id
			AND s.legacy_call_letters=h.legacy_call_letters
			AND s.start_time=h.start_time
			AND s.end_time=h.end_time
		JOIN #rating_requests rr ON 
			h.id = rr.id 
			AND h.legacy_call_letters=rr.legacy_call_letters
	GROUP BY
		h.id, h.legacy_call_letters

	IF OBJECT_ID('tempdb..#rating_requests') IS NOT NULL DROP TABLE #rating_requests;
	IF OBJECT_ID('tempdb..#audience_ids') IS NOT NULL DROP TABLE #audience_ids;
	IF OBJECT_ID('tempdb..#hut_market_codes') IS NOT NULL DROP TABLE #hut_market_codes;
	IF OBJECT_ID('tempdb..#usage_days') IS NOT NULL DROP TABLE #usage_days;
	IF OBJECT_ID('tempdb..#usage') IS NOT NULL DROP TABLE #usage;
	IF OBJECT_ID('tempdb..#share_market_codes') IS NOT NULL DROP TABLE #share_market_codes;
	IF OBJECT_ID('tempdb..#share_usage_days') IS NOT NULL DROP TABLE #share_usage_days;
	IF OBJECT_ID('tempdb..#share_usages') IS NOT NULL DROP TABLE #share_usages;
	IF OBJECT_ID('tempdb..#share_viewer_days') IS NOT NULL DROP TABLE #share_viewer_days;
	IF OBJECT_ID('tempdb..#share_viewers') IS NOT NULL DROP TABLE #share_viewers;
	IF OBJECT_ID('tempdb..#share') IS NOT NULL DROP TABLE #share;
END

GO






/*************************************** END BCOP-2088 *********************************************************/



/*************************************** END UPDATE SCRIPT *******************************************************/
------------------------------------------------------------------------------------------------------------------
------------------------------------------------------------------------------------------------------------------

-- Update the Schema Version of the database to the current release version
UPDATE system_component_parameters 
SET parameter_value = '5.8.12' -- Current release version
WHERE parameter_key = 'SchemaVersion'
GO

print 'Updated schema Version'

IF(XACT_STATE() = 1)
BEGIN
	
	IF EXISTS (SELECT TOP 1 * 
		FROM #previous_version 
		WHERE [version] = '5.8.11' -- Previous release version
		OR [version] = '5.8.12') -- Current release version
	BEGIN
		PRINT 'Database Successfully Updated'
		COMMIT TRANSACTION
		DROP TABLE #previous_version
	END
	ELSE
	BEGIN
		PRINT 'Incorrect Previous Database Version'
		ROLLBACK TRANSACTION
	END

END
GO

IF(XACT_STATE() = -1)
BEGIN
	ROLLBACK TRANSACTION
	PRINT 'Database Update Failed. Transaction rolled back.'
END
GO

















