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

/* START: External Ratings Removal */
IF  EXISTS (SELECT * FROM sys.synonyms s JOIN sys.schemas sc ON sc.schema_id=s.schema_id WHERE s.name='distributor_headers' AND sc.name='nsi')
	DROP SYNONYM [nsi].[distributor_headers];
IF  EXISTS (SELECT * FROM sys.synonyms s JOIN sys.schemas sc ON sc.schema_id=s.schema_id WHERE s.name='market_headers' AND sc.name='nsi')
	DROP SYNONYM [nsi].[market_headers]
IF  EXISTS (SELECT * FROM sys.synonyms s JOIN sys.schemas sc ON sc.schema_id=s.schema_id WHERE s.name='market_universe_estimate_header_audiences' AND sc.name='nsi')
	DROP SYNONYM [nsi].[market_universe_estimate_header_audiences]
IF  EXISTS (SELECT * FROM sys.synonyms s JOIN sys.schemas sc ON sc.schema_id=s.schema_id WHERE s.name='market_universe_estimate_headers' AND sc.name='nsi')
	DROP SYNONYM [nsi].[market_universe_estimate_headers]
IF  EXISTS (SELECT * FROM sys.synonyms s JOIN sys.schemas sc ON sc.schema_id=s.schema_id WHERE s.name='quarter_hour_distributor_estimate_audiences' AND sc.name='nsi')
	DROP SYNONYM [nsi].[quarter_hour_distributor_estimate_audiences]
IF  EXISTS (SELECT * FROM sys.synonyms s JOIN sys.schemas sc ON sc.schema_id=s.schema_id WHERE s.name='quarter_hour_distributor_estimates' AND sc.name='nsi')
	DROP SYNONYM [nsi].[quarter_hour_distributor_estimates]
IF  EXISTS (SELECT * FROM sys.synonyms s JOIN sys.schemas sc ON sc.schema_id=s.schema_id WHERE s.name='quarter_hour_estimate_audiences' AND sc.name='nsi')
	DROP SYNONYM [nsi].[quarter_hour_estimate_audiences]
IF  EXISTS (SELECT * FROM sys.synonyms s JOIN sys.schemas sc ON sc.schema_id=s.schema_id WHERE s.name='quarter_hour_estimates' AND sc.name='nsi')
	DROP SYNONYM [nsi].[quarter_hour_estimates]
GO

IF NOT EXISTS (SELECT * FROM sys.objects where object_id = OBJECT_ID(N'[nsi].[hwc_universes]') AND type in (N'U'))
BEGIN
	/****** Object:  Table [nsi].[hwc_universes]    Script Date: 2/20/2018 2:06:54 PM ******/
	CREATE TABLE [nsi].[hwc_universes](
		[media_month_id] [smallint] NOT NULL,
		[market_code] [smallint] NOT NULL,
		[audience_id] [int] NOT NULL,
		[universe] [float] NOT NULL,
	 CONSTRAINT [PK_hwc_universes] PRIMARY KEY CLUSTERED 
	(
		[media_month_id] ASC,
		[market_code] ASC,
		[audience_id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects where object_id = OBJECT_ID(N'[nsi].[market_headers]') AND type in (N'U'))
BEGIN
	/****** Object:  Table [nsi].[market_headers]    Script Date: 1/18/2018 3:45:58 PM ******/
	CREATE TABLE [nsi].[market_headers](
		[media_month_id] [int] NOT NULL,
		[file_name] [varchar](255) NOT NULL,
		[dma_id] [int] NULL,
		[format_version] [varchar](15) NOT NULL,
		[market_code] [smallint] NOT NULL,
		[dma_code] [smallint] NOT NULL,
		[market_rank] [smallint] NULL,
		[geography_indicator] [varchar](15) NOT NULL,
		[geography_name] [varchar](31) NOT NULL,
		[start_datetime_of_survey] [datetime] NOT NULL,
		[end_datetime_of_survey] [datetime] NOT NULL,
		[number_of_days_in_survey] [smallint] NOT NULL,
		[number_of_weeks_in_survey] [smallint] NOT NULL,
		[reporting_day_start_time] [time](7) NOT NULL,
		[reporting_service] [varchar](1) NOT NULL,
		[special_report_exclusion_indicator] [varchar](7) NOT NULL,
		[subsample_indicator] [varchar](7) NOT NULL,
		[header_sample_type] [varchar](1) NOT NULL,
		[reissued] [bit] NOT NULL,
		[data_accredited] [bit] NOT NULL,
		[playback_type] [varchar](1) NOT NULL,
		[time_interval] [int] NOT NULL,
		[collection_method] [varchar](1) NOT NULL,
		[contest_indicator] [bit] NOT NULL,
		[report_period] [varchar](7) NOT NULL,
		[report_year] [smallint] NOT NULL,
		[market_time_zone] [smallint] NOT NULL,
		[market_class_code] [varchar](1) NULL,
		[distributor_or_market_exclusion_indicator] [bit] NOT NULL,
		[daylight_savings_time_indicator] [bit] NOT NULL,
	 CONSTRAINT [PK_nsi_market_headers] PRIMARY KEY CLUSTERED 
	(
		[media_month_id] ASC,
		[file_name] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects where object_id = OBJECT_ID(N'[nsi].[post_months]') AND type in (N'U'))
BEGIN
	/****** Object:  Table [nsi].[post_months]    Script Date: 1/18/2018 3:52:23 PM ******/
	CREATE TABLE [nsi].[post_months](
		[media_month_id] [int] NOT NULL,
		[num_markets] [int] NOT NULL,
		[universes] [bit] NOT NULL,
		[usages] [bit] NOT NULL,
		[viewers] [bit] NOT NULL,
		[date_crunched] [datetime] NOT NULL,
	 CONSTRAINT [PK_post_months] PRIMARY KEY CLUSTERED 
	(
		[media_month_id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects where object_id = OBJECT_ID(N'[nsi].[codes]') AND type in (N'U'))
BEGIN
	/****** Object:  Table [nsi].[codes]    Script Date: 1/19/2018 2:57:27 PM ******/
	CREATE TABLE [nsi].[codes](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[code] [varchar](15) NOT NULL,
		[name] [varchar](63) NOT NULL,
		[code_set] [varchar](31) NOT NULL,
	 CONSTRAINT [PK_sample_type] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

	INSERT INTO nsi.codes SELECT '1','Total DMA','Sample Type'
	INSERT INTO nsi.codes SELECT '2','Hard-Wired Cable','Sample Type'
	INSERT INTO nsi.codes SELECT '3','Both Total DMA & Hard-wired Cable','Sample Type'
	INSERT INTO nsi.codes SELECT '4','Station Totals Only (Non-DMA) ','Sample Type'
	INSERT INTO nsi.codes SELECT '1','Set Meter ','Collection Method'
	INSERT INTO nsi.codes SELECT '2','LPM','Collection Method'
	INSERT INTO nsi.codes SELECT '3','LPM Preliminary','Collection Method'
	INSERT INTO nsi.codes SELECT '4','Diary Only','Collection Method'
	INSERT INTO nsi.codes SELECT 'O','Live Only (LPM markets only)','Playback Types'
	INSERT INTO nsi.codes SELECT 'S','Live + Same Day','Playback Types'
	INSERT INTO nsi.codes SELECT '1','Live + 1 (Live + 24 hours)','Playback Types'
	INSERT INTO nsi.codes SELECT '3','Live + 3 (Live + 75 hours)','Playback Types'
	INSERT INTO nsi.codes SELECT '7','Live + 7 (Live + 168 hours)','Playback Types'
	INSERT INTO nsi.codes SELECT '00','Weekly','Report Period'
	INSERT INTO nsi.codes SELECT '01','January','Report Period'
	INSERT INTO nsi.codes SELECT '02','February','Report Period'
	INSERT INTO nsi.codes SELECT '03','March','Report Period'
	INSERT INTO nsi.codes SELECT '04','April','Report Period'
	INSERT INTO nsi.codes SELECT '05','May','Report Period'
	INSERT INTO nsi.codes SELECT '06','June','Report Period'
	INSERT INTO nsi.codes SELECT '07','July','Report Period'
	INSERT INTO nsi.codes SELECT '08','August','Report Period'
	INSERT INTO nsi.codes SELECT '09','September','Report Period'
	INSERT INTO nsi.codes SELECT '10','October','Report Period'
	INSERT INTO nsi.codes SELECT '11','November','Report Period'
	INSERT INTO nsi.codes SELECT '12','December','Report Period'
	INSERT INTO nsi.codes SELECT '01','Eastern','Market Time Zone'
	INSERT INTO nsi.codes SELECT '02','Central','Market Time Zone'
	INSERT INTO nsi.codes SELECT '03','Mountain','Market Time Zone'
	INSERT INTO nsi.codes SELECT '04','Pacific','Market Time Zone'
	INSERT INTO nsi.codes SELECT '05','Alaskan','Market Time Zone'
	INSERT INTO nsi.codes SELECT '06','Hawaiian','Market Time Zone'
	INSERT INTO nsi.codes SELECT '1','NSI - Nielsen Station Index','Reporting Service'
	INSERT INTO nsi.codes SELECT '2','NHSI - Nielsen Hispanic Station Index','Reporting Service'
	INSERT INTO nsi.codes SELECT '3','NSI-Nielsen Station Index LPM Prelim Period','Reporting Service'
	INSERT INTO nsi.codes SELECT '4','NHSI-Nielsen Hispanic Station Index LPM Prelim Period','Reporting Service'
	INSERT INTO nsi.codes SELECT '5','NHIL - Nielsen Local Cable Service','Reporting Service'
	INSERT INTO nsi.codes SELECT 'B','Broadcast','Distribution Source Type'
	INSERT INTO nsi.codes SELECT 'C','Cable','Distribution Source Type'
	INSERT INTO nsi.codes SELECT '1','Local broadcast or cable station','Station Type Code'
	INSERT INTO nsi.codes SELECT '2','Parent station of any group','Station Type Code'
	INSERT INTO nsi.codes SELECT '5','Parent + Satellite/child station group','Station Type Code'
	INSERT INTO nsi.codes SELECT '8','Satellite station','Station Type Code'
	INSERT INTO nsi.codes SELECT '9','Outside station or cable net','Station Type Code'
	INSERT INTO nsi.codes SELECT 'V','Fully reportable in ViP report','Reportability Status'
	INSERT INTO nsi.codes SELECT 'D','Daypart only reportable in ViP report','Reportability Status'
	INSERT INTO nsi.codes SELECT 'M','Local monthlies only - not reportable in ViP report','Reportability Status'
	INSERT INTO nsi.codes SELECT 'N','No audience estimates','Reportability Status'
	INSERT INTO nsi.codes SELECT 'ABC','ABC Network','Program Sources'
	INSERT INTO nsi.codes SELECT 'AZA','Azteca America','Program Sources'
	INSERT INTO nsi.codes SELECT 'CBL','Cable Network','Program Sources'
	INSERT INTO nsi.codes SELECT 'CBS','CBS Network','Program Sources'
	INSERT INTO nsi.codes SELECT 'CW','The CW','Program Sources'
	INSERT INTO nsi.codes SELECT 'FOX','FOX Network','Program Sources'
	INSERT INTO nsi.codes SELECT 'ION','ION Media Networks','Program Sources'
	INSERT INTO nsi.codes SELECT 'L','Local','Program Sources'
	INSERT INTO nsi.codes SELECT 'LM','Local Movie','Program Sources'
	INSERT INTO nsi.codes SELECT 'LN','Local News','Program Sources'
	INSERT INTO nsi.codes SELECT 'LS','Local Sports','Program Sources'
	INSERT INTO nsi.codes SELECT 'MNT','My Network TV','Program Sources'
	INSERT INTO nsi.codes SELECT 'NBC','NBC Network','Program Sources'
	INSERT INTO nsi.codes SELECT 'PAX','Paxson Network','Program Sources'
	INSERT INTO nsi.codes SELECT 'PBS','PBS Network','Program Sources'
	INSERT INTO nsi.codes SELECT 'SYN','Syndicated','Program Sources'
	INSERT INTO nsi.codes SELECT 'T','Telemundo','Program Sources'
	INSERT INTO nsi.codes SELECT 'TF','Telefutura','Program Sources'
	INSERT INTO nsi.codes SELECT 'U','Univision','Program Sources'
	INSERT INTO nsi.codes SELECT 'UPN','UPN Network','Program Sources'
	INSERT INTO nsi.codes SELECT 'WBN','WB Network','Program Sources'
	INSERT INTO nsi.codes SELECT '6','SVIP - NSI LPM DMA Preview','Reporting Service'
	INSERT INTO nsi.codes SELECT '2','Monday','Program Day Code'
	INSERT INTO nsi.codes SELECT '3','Tuesday','Program Day Code'
	INSERT INTO nsi.codes SELECT '4','Wednesday','Program Day Code'
	INSERT INTO nsi.codes SELECT '5','Thursday','Program Day Code'
	INSERT INTO nsi.codes SELECT '6','Friday','Program Day Code'
	INSERT INTO nsi.codes SELECT '7','Saturday','Program Day Code'
	INSERT INTO nsi.codes SELECT '8','Sunday','Program Day Code'
	INSERT INTO nsi.codes SELECT '5','Set Meter Hybrid','Collection Method'
	INSERT INTO nsi.codes SELECT '6','Set Meter Hybrid Prelim','Collection Method'
	INSERT INTO nsi.codes SELECT '7','LPM Hybrid','Collection Method'
	INSERT INTO nsi.codes SELECT '8','LPM Hybrid Prelim','Collection Method'
	INSERT INTO nsi.codes SELECT '9','Diary Hybrid (RPD Only)','Collection Method'
	INSERT INTO nsi.codes SELECT '10','Diary Hybrid Prelim (RPD Only)','Collection Method'
	INSERT INTO nsi.codes SELECT '11','Diary Hybrid (RPD+CR)','Collection Method'
	INSERT INTO nsi.codes SELECT '12','Diary Hybrid Prelim (RPD+CR)','Collection Method'
	INSERT INTO nsi.codes SELECT '7','NSIX-Nielsen Station Index Hybrid Prelim Period','Reporting Service'
END
GO




IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[nsi].[usp_ForecastNsiRatingsMonth]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [nsi].[usp_ForecastNsiRatingsMonth];
GO

IF OBJECT_ID('udf_GetNSICodesForCodeSet') IS NULL
BEGIN
  EXEC('/****** Object:  UserDefinedFunction [dbo].[udf_GetNSICodesForCodeSet]    Script Date: 1/19/2018 3:00:17 PM ******/
	-- =============================================
	-- Author:		David Sisson
	-- Create date: 08/31/2011
	-- Description:	Returns NSI code translations for the given code set
	-- =============================================
	CREATE FUNCTION [dbo].[udf_GetNSICodesForCodeSet] 
	(	
		@code_set varchar(31)
	)
	RETURNS TABLE 
	AS
	RETURN 
	(
		SELECT 
			id, 
			rtrim(ltrim(code)) code, 
			rtrim(ltrim(name)) name, 
			rtrim(ltrim(code_set)) code_set
		from
			nsi.codes with(nolock)
		where
			rtrim(ltrim(code_set)) = rtrim(ltrim(@code_set))
	)');
END;
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_getDmaBookForDmaCode]') AND type in (N'P', N'PC'))
BEGIN
	EXEC sp_executesql @statement = N'CREATE PROCEDURE [dbo].[usp_getDmaBookForDmaCode] AS'
END
GO
/****** Object:  StoredProcedure [dbo].[usp_getDmaBookForDmaCode]    Script Date: 1/19/2018 12:46:25 PM ******/
-- usp_getDmaBookForDmaCode 2011,11,563
ALTER PROCEDURE [dbo].[usp_getDmaBookForDmaCode]
	@reportyear as int,
	@reportperiod as int,
	@dma_code as int
AS
BEGIN
		SELECT      
			TOP 1 mh.report_period,
			mh.report_year,
			CASE
					CASE mh.playback_type
						WHEN 'O' THEN 0
						WHEN 'S' THEN 1
						WHEN '1' THEN 2
						WHEN '3' THEN 3
						WHEN '7' THEN 4
					END
                  
					WHEN 0 THEN 'Live Only'
					WHEN 1 THEN 'Live+SD'
					WHEN 2 THEN 'Live+1'
					WHEN 3 THEN 'Live+3'
					WHEN 4 THEN 'Live+7'
			END playback_type,
			CASE mh.playback_type
					WHEN 'O' THEN 0
					WHEN 'S' THEN 1
					WHEN '1' THEN 2
					WHEN '3' THEN 3
					WHEN '7' THEN 4
			END pbnum
		FROM
			nsi.market_headers mh (NOLOCK)
			JOIN udf_GetNSICodesForCodeSet('Reporting Service') c_rs ON
					mh.reporting_service = c_rs.code
		WHERE
			report_year <= @reportyear
			AND CAST(report_period AS INT) <= @reportperiod
			AND dma_code = @dma_code
			AND 'NSI - Nielsen Station Index' = c_rs.name
		ORDER BY 
			mh.report_year DESC, 
			mh.report_period DESC,
			pbnum DESC
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_getDmaBookForDmaCode]') AND type in (N'P', N'PC'))
BEGIN
	EXEC sp_executesql @statement = N'CREATE PROCEDURE [dbo].[usp_getDmaBookForDmaCode] AS'
END
GO	
/****** Object:  StoredProcedure [dbo].[usp_ARS_GetDMABooks]    Script Date: 1/19/2018 12:46:25 PM ******/
CREATE PROCEDURE [dbo].[usp_ARS_GetDMABooks]
	@reportyear as int,
	@reportperiod as int,
	@dma_code as int
AS
BEGIN
		SELECT DISTINCT 
		report_period, 
		report_year
		FROM
		nsi.market_headers WITH (NOLOCK) 
		ORDER BY
		report_year desc, 
		report_period desc
END
GO

/* END: External Ratings Removal */

/*************************************** END UPDATE SCRIPT *******************************************************/
------------------------------------------------------------------------------------------------------------------
------------------------------------------------------------------------------------------------------------------

-- Update the Schema Version of the database to the current release version
UPDATE system_component_parameters 
SET parameter_value = '18.04.1' -- Current release version
WHERE parameter_key = 'SchemaVersion'
GO

print 'Updated schema Version'

IF(XACT_STATE() = 1)
BEGIN
	
	IF EXISTS (SELECT TOP 1 * 
		FROM #previous_version 
		WHERE [version] = '18.03.1' -- Previous release version
		OR [version] = '18.04.1') -- Current release version
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