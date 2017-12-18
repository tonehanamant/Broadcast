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

/*************************************** BCOP-2155 ***************************************************************/

IF OBJECT_ID('station_inventory_spots', 'U') IS NULL
BEGIN
	CREATE TABLE station_inventory_spots
	(
		id INT IDENTITY(1,1) NOT NULL,
		proposal_version_detail_quarter_week_id INT NULL,
		station_inventory_manifest_id INT NOT NULL,
		media_week_id INT NOT NULL,
		inventory_lost BIT NOT NULL,
		overridden_impressions FLOAT NULL,
		overridden_rate MONEY NULL,
		delivery_cpm MONEY NULL,
		CONSTRAINT [PK_station_inventory_spots] PRIMARY KEY CLUSTERED
		(
			id ASC
		) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	)

	ALTER TABLE [dbo].[station_inventory_spots]  WITH CHECK ADD CONSTRAINT [FK_station_inventory_spots_proposal_version_detail_quarter_weeks] FOREIGN KEY(proposal_version_detail_quarter_week_id)
	REFERENCES [dbo].[proposal_version_detail_quarter_weeks] ([id])
	ON DELETE CASCADE

	ALTER TABLE [dbo].[station_inventory_spots] CHECK CONSTRAINT [FK_station_inventory_spots_proposal_version_detail_quarter_weeks]

	ALTER TABLE [dbo].[station_inventory_spots]  WITH CHECK ADD CONSTRAINT [FK_station_inventory_spots_station_inventory_manifest] FOREIGN KEY(station_inventory_manifest_id)
	REFERENCES [dbo].[station_inventory_manifest] ([id])
	ON DELETE CASCADE

	ALTER TABLE [dbo].[station_inventory_spots] CHECK CONSTRAINT [FK_station_inventory_spots_station_inventory_manifest]
END

IF OBJECT_ID('station_inventory_spot_audiences', 'U') IS NULL
BEGIN
	CREATE TABLE station_inventory_spot_audiences
	(
		station_inventory_spot_id INT NOT NULL,
		audience_id INT NOT NULL,
		calculated_impressions FLOAT NULL,
		calculated_rate MONEY NULL,
		CONSTRAINT [PK_station_inventory_spot_audiences] PRIMARY KEY CLUSTERED
		(
		  station_inventory_spot_id ASC,
		  audience_id ASC
		) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	)

	ALTER TABLE [dbo].[station_inventory_spot_audiences]  WITH CHECK ADD CONSTRAINT [FK_station_inventory_spot_audiences_station_inventory_spots] FOREIGN KEY(station_inventory_spot_id)
	REFERENCES [dbo].[station_inventory_spots] (id)
	ON DELETE CASCADE

	ALTER TABLE [dbo].[station_inventory_spot_audiences] CHECK CONSTRAINT [FK_station_inventory_spot_audiences_station_inventory_spots]

	ALTER TABLE [dbo].[station_inventory_spot_audiences]  WITH CHECK ADD CONSTRAINT [FK_station_inventory_spot_audiences_audiences] FOREIGN KEY(audience_id)
	REFERENCES [dbo].[audiences] ([id])
	ON DELETE CASCADE

	ALTER TABLE [dbo].[station_inventory_spot_audiences] CHECK CONSTRAINT [FK_station_inventory_spot_audiences_audiences]
END

/*************************************** BCOP-2155 ***************************************************************/

/*************************************** END UPDATE SCRIPT *******************************************************/

------------------------------------------------------------------------------------------------------------------
------------------------------------------------------------------------------------------------------------------

-- Update the Schema Version of the database to the current release version
UPDATE system_component_parameters 
SET parameter_value = '18.01.1' -- Current release version
WHERE parameter_key = 'SchemaVersion'
GO

print 'Updated schema Version'

IF(XACT_STATE() = 1)
BEGIN
	
	IF EXISTS (SELECT TOP 1 * 
		FROM #previous_version 
		WHERE [version] = '5.8.11' -- Previous release version
		OR [version] = '18.01.1') -- Current release version
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


