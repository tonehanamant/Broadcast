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
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS (NOLOCK) WHERE table_name='market_headers' AND column_name='collection_method' AND CHARACTER_MAXIMUM_LENGTH=1)
BEGIN
       SELECT * INTO temp_db_backup.dbo.market_headers_backup FROM nsi.market_headers;

       /****** Object:  Table [nsi].[market_headers]    Script Date: 9/12/2018 3:16:19 PM ******/
       DROP TABLE [nsi].[market_headers];

       /****** Object:  Table [nsi].[market_headers]    Script Date: 9/12/2018 3:16:19 PM ******/
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
              [collection_method] [varchar](2) NOT NULL,
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

       INSERT INTO [nsi].[market_headers]
              SELECT * FROM temp_db_backup.dbo.market_headers_backup;

       DROP TABLE temp_db_backup.dbo.market_headers_backup;
END
GO

IF (SELECT COUNT(1) FROM nsi.codes WHERE code_set='Collection Method') <> 18
BEGIN
       DELETE FROM nsi.codes WHERE code_set='Collection Method';

       INSERT INTO nsi.codes (code,name,code_set) VALUES ('1','Set Meter','Collection Method');
       INSERT INTO nsi.codes (code,name,code_set) VALUES ('2','LPM','Collection Method');
       INSERT INTO nsi.codes (code,name,code_set) VALUES ('3','LPM Preliminary','Collection Method');
       INSERT INTO nsi.codes (code,name,code_set) VALUES ('4','Diary only','Collection Method');
       INSERT INTO nsi.codes (code,name,code_set) VALUES ('5','Set Meter Parallel/Impact Data','Collection Method');
       INSERT INTO nsi.codes (code,name,code_set) VALUES ('6','Code Reader','Collection Method');
       INSERT INTO nsi.codes (code,name,code_set) VALUES ('7','Code Reader Parallel/Impact Data','Collection Method');
       INSERT INTO nsi.codes (code,name,code_set) VALUES ('8','LPM + PPM Parallel/Impact Data','Collection Method');
       INSERT INTO nsi.codes (code,name,code_set) VALUES ('9','Set Meter + PPM Parallel/Impact Data','Collection Method');
       INSERT INTO nsi.codes (code,name,code_set) VALUES ('10','Set Meter + RPD Parallel/Impact Data','Collection Method');
       INSERT INTO nsi.codes (code,name,code_set) VALUES ('11','Code Reader + RPD Parallel/Impact Data','Collection Method');
       INSERT INTO nsi.codes (code,name,code_set) VALUES ('12','RPD Plus Parallel/Impact Data','Collection Method');
       INSERT INTO nsi.codes (code,name,code_set) VALUES ('13','Set Meter+ PPM+RPD Parallel/Impact Data','Collection Method');
       INSERT INTO nsi.codes (code,name,code_set) VALUES ('20','LPM + PPM','Collection Method');
       INSERT INTO nsi.codes (code,name,code_set) VALUES ('30','Set Meter + PPM','Collection Method');
       INSERT INTO nsi.codes (code,name,code_set) VALUES ('40','Set Meter + RPD','Collection Method');
       INSERT INTO nsi.codes (code,name,code_set) VALUES ('50','Code Reader + RPD','Collection Method');
       INSERT INTO nsi.codes (code,name,code_set) VALUES ('60','RPD Plus','Collection Method');
END


/*************************************** END UPDATE SCRIPT *******************************************************/
------------------------------------------------------------------------------------------------------------------
------------------------------------------------------------------------------------------------------------------

-- Update the Schema Version of the database to the current release version
UPDATE system_component_parameters 
SET parameter_value = '18.10.1' -- Current release version
WHERE parameter_key = 'SchemaVersion'
GO

print 'Updated schema Version'

IF(XACT_STATE() = 1)
BEGIN
	
	IF EXISTS (SELECT TOP 1 * 
		FROM #previous_version 
		WHERE [version] = '18.09.1' -- Previous release version
		OR [version] = '18.10.1') -- Current release version
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