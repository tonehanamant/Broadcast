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


/*************************************** START UPDATE SCRIPT *****************************************************/


/*************************************** BCOP-3515 *****************************************************/

IF OBJECT_ID('proposal_version_detail_proprietary_pricing', 'U') IS NULL
BEGIN
	CREATE TABLE proposal_version_detail_proprietary_pricing
	(
		proposal_version_detail_id INT NOT NULL,
		inventory_source TINYINT NOT NULL,
		impressions_balance FLOAT NOT NULL,
		cpm MONEY NOT NULL,
		CONSTRAINT [PK_proposal_version_detail_proprietary_pricing] PRIMARY KEY CLUSTERED
		(
			proposal_version_detail_id, inventory_source ASC
		) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]		
	)

	ALTER TABLE [dbo].[proposal_version_detail_proprietary_pricing]
	WITH CHECK ADD CONSTRAINT [FK_proposal_version_detail_proprietary_pricing_proposal_version_details] 
	FOREIGN KEY(proposal_version_detail_id)
	REFERENCES [dbo].[proposal_version_details] ([id])
	ON DELETE CASCADE

	ALTER TABLE [dbo].[proposal_version_detail_proprietary_pricing] CHECK CONSTRAINT [FK_proposal_version_detail_proprietary_pricing_proposal_version_details]
END

/*************************************** BCOP-3515 - END *****************************************************/

/*************************************** START BCOP-3517 *****************************************************/
IF OBJECT_ID('station_inventory_spot_genres', 'U') IS NULL
BEGIN
	CREATE TABLE [station_inventory_spot_genres](
		id INT IDENTITY(1,1) NOT NULL,
		station_inventory_spot_id INT NOT NULL,
		genre_id INT NOT NULL
		CONSTRAINT [PK_station_inventory_spot_genres] PRIMARY KEY CLUSTERED
		(
			id ASC
		) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	)

	ALTER TABLE [dbo].[station_inventory_spot_genres]  WITH CHECK ADD  CONSTRAINT [FK_station_inventory_spot_genres_station_inventory_spots] FOREIGN KEY([station_inventory_spot_id])
	REFERENCES [dbo].[station_inventory_spots] ([id])
	ON DELETE CASCADE
	ALTER TABLE [dbo].[station_inventory_spot_genres] CHECK CONSTRAINT [FK_station_inventory_spot_genres_station_inventory_spots]

	ALTER TABLE [dbo].[station_inventory_spot_genres]  WITH CHECK ADD  CONSTRAINT [FK_station_inventory_spot_genres_genres] FOREIGN KEY([genre_id])
	REFERENCES [dbo].[genres] ([id])
	ALTER TABLE [dbo].[station_inventory_spot_genres] CHECK CONSTRAINT [FK_station_inventory_spot_genres_genres]

	CREATE INDEX IX_station_inventory_spot_genres_genre_id ON [station_inventory_spot_genres] ([genre_id]);
	CREATE INDEX IX_station_inventory_spot_genres_station_inventory_spot_id ON [station_inventory_spot_genres] ([station_inventory_spot_id]);
END
GO

/*************************************** END BCOP-3517 *****************************************************/

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