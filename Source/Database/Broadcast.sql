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

-- Only run this script when the schema is in the correct previous version
INSERT INTO #previous_version
		SELECT parameter_value 
		FROM system_component_parameters 
		WHERE parameter_key = 'SchemaVersion' 
GO

/*************************************** START UPDATE SCRIPT *****************************************************/

/*************************************** START PRI-10290 *****************************************************/

IF (EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_SCHEMA = 'dbo' AND 
				      TABLE_NAME = 'scx_generation_job_request_units'))
BEGIN
	DROP TABLE scx_generation_job_request_units
END

IF (EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_SCHEMA = 'dbo' AND 
				      TABLE_NAME = 'scx_generation_job_requests'))
BEGIN
	DROP TABLE scx_generation_job_requests
END

-- If the table doesn't have the new column, recreate it and its dependencies.
IF NOT EXISTS(SELECT 1 FROM sys.columns 
              WHERE NAME = 'inventory_source_id' AND
              OBJECT_ID = OBJECT_ID('scx_generation_jobs'))
BEGIN
	DROP TABLE scx_generation_job_files
	DROP TABLE scx_generation_job_units
	DROP TABLE scx_generation_jobs
END

-- Renaming incorrectly named PK.
IF EXISTS (SELECT * 
		   FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
		   WHERE CONSTRAINT_NAME ='PK_scx_generation_files')
BEGIN
	EXEC sp_rename @objname = N'[dbo].[scx_generation_job_files].[PK_scx_generation_files]', @newname = N'PK_scx_generation_job_files'
END

IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_SCHEMA = 'dbo' AND 
				      TABLE_NAME = 'scx_generation_jobs'))
BEGIN
	CREATE TABLE [dbo].[scx_generation_jobs](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[inventory_source_id] [int] NOT NULL,
		[daypart_code_id] [int] NOT NULL,
		[start_date] [datetime] NOT NULL,
		[end_date] [datetime] NOT NULL,
		[status] [int] NOT NULL,
		[queued_at] [datetime] NOT NULL,
		[completed_at] [datetime] NULL,
		[requested_by] varchar(63) NOT NULL,
	 CONSTRAINT [PK_scx_generation_jobs] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[scx_generation_jobs] WITH CHECK ADD  CONSTRAINT [FK_scx_generation_jobs_inventory_sources] FOREIGN KEY([inventory_source_id])
	REFERENCES [dbo].[inventory_sources] ([id])
	
	ALTER TABLE [dbo].[scx_generation_jobs] CHECK CONSTRAINT [FK_scx_generation_jobs_inventory_sources]
	
	ALTER TABLE [dbo].[scx_generation_jobs] WITH CHECK ADD CONSTRAINT [FK_scx_generation_jobs_daypart_codes] FOREIGN KEY([daypart_code_id])
	REFERENCES [dbo].[daypart_codes] ([id])
	
	ALTER TABLE [dbo].[scx_generation_jobs] CHECK CONSTRAINT [FK_scx_generation_jobs_daypart_codes]
END

-- [scx_generation_job_request_units]
IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_SCHEMA = 'dbo' AND 
				      TABLE_NAME = 'scx_generation_job_units'))
BEGIN
	CREATE TABLE [dbo].[scx_generation_job_units](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[scx_generation_job_id] [int] NOT NULL,
		[unit_name] varchar(50) NOT NULL
	 CONSTRAINT [PK_scx_generation_job_units] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[scx_generation_job_units]  WITH CHECK ADD  CONSTRAINT [FK_scx_generation_job_units_scx_generation_jobs] FOREIGN KEY([scx_generation_job_id])
	REFERENCES [dbo].[scx_generation_jobs] ([id])

	ALTER TABLE [dbo].[scx_generation_job_units] CHECK CONSTRAINT [FK_scx_generation_job_units_scx_generation_jobs]
END

--[scx_generation_job_files]
IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_SCHEMA = 'dbo' AND 
				      TABLE_NAME = 'scx_generation_job_files'))
BEGIN
	CREATE TABLE [dbo].[scx_generation_job_files](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[scx_generation_job_id] [int] NOT NULL,
		[file_name] varchar(255) NOT NULL,
		[inventory_source_id] [int] NOT NULL,
		[daypart_code_id] [int] NOT NULL,
		[start_date] [datetime] NOT NULL,
		[end_date] [datetime] NOT NULL,
		[unit_name] varchar(50) NOT NULL
	 CONSTRAINT [PK_scx_generation_job_files] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[scx_generation_job_files]  WITH CHECK ADD  CONSTRAINT [FK_scx_generation_job_files_scx_generation_job] FOREIGN KEY([scx_generation_job_id])
	REFERENCES [dbo].[scx_generation_jobs] ([id])
	
	ALTER TABLE [dbo].[scx_generation_job_files] CHECK CONSTRAINT [FK_scx_generation_job_files_scx_generation_job]
	
	ALTER TABLE [dbo].[scx_generation_job_files]  WITH CHECK ADD  CONSTRAINT [FK_scx_generation_job_files_inventory_sources] FOREIGN KEY([inventory_source_id])
	REFERENCES [dbo].[inventory_sources] ([id])
	
	ALTER TABLE [dbo].[scx_generation_job_files] CHECK CONSTRAINT [FK_scx_generation_job_files_inventory_sources]
	
	ALTER TABLE [dbo].[scx_generation_job_files]  WITH CHECK ADD  CONSTRAINT [FK_scx_generation_job_files_daypart_codes] FOREIGN KEY([daypart_code_id])
	REFERENCES [dbo].[daypart_codes] ([id])
	
	ALTER TABLE [dbo].[scx_generation_job_files] CHECK CONSTRAINT [FK_scx_generation_job_files_daypart_codes]
END

/*************************************** END PRI-10290 *****************************************************/


/*************************************** END UPDATE SCRIPT *******************************************************/

-- Update the Schema Version of the database to the current release version
UPDATE system_component_parameters 
SET parameter_value = '19.09.1' -- Current release version
WHERE parameter_key = 'SchemaVersion'
GO

print 'Updated schema Version'

IF(XACT_STATE() = 1)
BEGIN 
	
	IF EXISTS (SELECT TOP 1 * 
		FROM #previous_version 
		WHERE [version] = '19.08.1' -- Previous release version
		OR [version] = '19.09.1') -- Current release version
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