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

/************************ START BCOP3889 **************************************************************************************/
/************************ START BCOP3997 **************************************************************************************/

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


GO
IF EXISTS(select * FROM sys.views where name = 'uvw_market_codes_call_letters')
BEGIN
	DROP VIEW [nsi].[uvw_market_codes_call_letters]
END

GO

CREATE VIEW [nsi].[uvw_market_codes_call_letters]
WITH SCHEMABINDING
AS
       SELECT
              v.media_month_id,
              v.legacy_call_letters,
              v.market_code,             
              COUNT_BIG(*) 'count'
       FROM
              [nsi].[viewers] v
       GROUP BY
              v.media_month_id,
              v.legacy_call_letters,
              v.market_code
              
GO

IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_uvw_market_codes_call_letters' AND object_id = OBJECT_ID('nsi.uvw_market_codes_call_letters'))
BEGIN
	DROP INDEX IX_uvw_market_codes_call_letters ON [nsi].[uvw_market_codes_call_letters]
END

GO 

CREATE UNIQUE CLUSTERED INDEX [IX_uvw_market_codes_call_letters] ON [nsi].[uvw_market_codes_call_letters]
(
       media_month_id ASC,
       legacy_call_letters ASC,
       market_code ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO

IF EXISTS ( SELECT  * FROM    sys.objects WHERE   object_id = OBJECT_ID(N'nsi.usp_GetImpressionsForMultiplePrograms_Daypart_Averages_Test'))
BEGIN
	DROP PROCEDURE [nsi].[usp_GetImpressionsForMultiplePrograms_Daypart_Averages_Test]
END
                   
GO


IF EXISTS ( SELECT  * FROM    sys.objects WHERE   object_id = OBJECT_ID(N'nsi.usp_GetImpressionsForMultiplePrograms_Daypart_Averages'))
BEGIN
	DROP PROCEDURE [nsi].[usp_GetImpressionsForMultiplePrograms_Daypart_Averages]
END
                   
GO
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

	CREATE TABLE #viewers (id INT NOT NULL, legacy_call_letters varchar(15) NOT NULL, audience_id INT NOT NULL, market_code SMALLINT NOT NULL, start_time INT NOT NULL, end_time INT NOT NULL, viewers FLOAT  
		PRIMARY KEY CLUSTERED(id, legacy_call_letters, audience_id, market_code, start_time, end_time))

	INSERT INTO #viewers
	SELECT
		rr.id, rr.legacy_call_letters,a.audience_id,v.market_code,v.start_time, v.end_time,
		-- basically if weekday and weekend then avg the two, otherwise return one or other
		CASE WHEN rr.mon=1 OR rr.tue=1 OR rr.wed=1 OR rr.thu=1 OR rr.fri=1 THEN 
			CASE WHEN rr.sat=1 OR rr.sun=1 THEN (vd.weekend_viewers  + vd.weekday_viewers) / 2 
				ELSE vd.weekday_viewers  
			END  			
		ELSE 
			CASE WHEN rr.sat=1 OR rr.sun=1 THEN vd.weekend_viewers ELSE 0 END 
		END 
	FROM
		#rating_requests rr
        JOIN nsi.uvw_market_codes_call_letters mccl ON mccl.media_month_id=@posting_media_month_id
            AND mccl.legacy_call_letters=rr.legacy_call_letters
		JOIN nsi.viewers v (NOLOCK) ON v.media_month_id=@posting_media_month_id
			AND v.legacy_call_letters=rr.legacy_call_letters
			AND v.market_code=mccl.market_code
			AND (v.start_time<=rr.end_time AND v.end_time>=rr.start_time)
		JOIN #min_playback_types mpt ON mpt.market_code=v.market_code
		CROSS APPLY #audience_ids a
		JOIN nsi.viewer_details vd (NOLOCK) ON vd.media_month_id=v.media_month_id
			AND vd.viewer_id=v.id
			AND vd.audience_id=a.audience_id
			AND vd.playback_type=mpt.available_playback_type

	--SELECT
	--	id,legacy_call_letters,audience_id,market_code,
	--	viewers--SUM(viewers) 'impressions', avg(viewers)
	--	,IsWeekday
	--FROM #viewers
	----GROUP BY 		id,legacy_call_letters,market_code,audience_id

	SELECT 
		market_averages.id,
		legacy_call_letters,
		audience_id, 
		SUM(market_impression_avg) impressions	-- sum of market averages by call letter, audieince, etc
	from 
	(	-- get averages of impressions for each market
		SELECT id,
				legacy_call_letters,
				market_code, 
				audience_id, 
				avg(viewers) market_impression_avg
		FROM 
			#viewers
		GROUP BY 
			id,legacy_call_letters,market_code, audience_id
	) as market_averages
	group by 
		market_averages.id,legacy_call_letters, audience_id

	IF OBJECT_ID('tempdb..#rating_requests') IS NOT NULL DROP TABLE #rating_requests;
	IF OBJECT_ID('tempdb..#min_playback_types') IS NOT NULL DROP TABLE #min_playback_types;
	IF OBJECT_ID('tempdb..#audience_ids') IS NOT NULL DROP TABLE #audience_ids;
	IF OBJECT_ID('tempdb..#viewers') IS NOT NULL DROP TABLE #viewers;
END
GO

/*************** END BCOP3997 **************************************************************************************/
/*************** END BCOP3889 **************************************************************************************/

GO

GO
/*************************************** END UPDATE SCRIPT *******************************************************/
------------------------------------------------------------------------------------------------------------------
------------------------------------------------------------------------------------------------------------------

-- Update the Schema Version of the database to the current release version
UPDATE system_component_parameters 
SET parameter_value = '19.01.1' -- Current release version
WHERE parameter_key = 'SchemaVersion'
GO

print 'Updated schema Version'

IF(XACT_STATE() = 1)
BEGIN
	
	IF EXISTS (SELECT TOP 1 * 
		FROM #previous_version 
		WHERE [version] = '18.12.1' -- Previous release version
		OR [version] = '19.01.1') -- Current release version
	BEGIN
		PRINT 'Database Successfully Updated'
		COMMIT TRANSACTION
		DROP TABLE #previous_version
	END
	ELSE
	BEGIN
		ROLLBACK TRANSACTION
		RAISERROR('Incorrect Previous Database Version', 11, 1)
	END

END
GO

IF(XACT_STATE() = -1)
BEGIN
	ROLLBACK TRANSACTION
	RAISERROR('Database Update Failed. Transaction rolled back.', 11, 1)
END
GO