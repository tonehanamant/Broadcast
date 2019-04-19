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

/*************************************** START PRI-6133 *****************************************************/
--add DIGI daypart code
IF NOT EXISTS(SELECT * FROM daypart_codes WHERE [name] = 'DIGI')
BEGIN
	INSERT INTO daypart_codes([name], is_active) VALUES('DIGI', 1)
END

--add Diginet sources
IF NOT EXISTS (SELECT * FROM inventory_sources WHERE [name] = 'Antenna TV')
BEGIN
	INSERT INTO inventory_sources([name], is_active, inventory_source_type) VALUES('Antenna TV', 1, 5)
END

IF NOT EXISTS (SELECT * FROM inventory_sources WHERE [name] = 'Bounce')
BEGIN
	INSERT INTO inventory_sources([name], is_active, inventory_source_type) VALUES('Bounce', 1, 5)
END

IF NOT EXISTS (SELECT * FROM inventory_sources WHERE [name] = 'BUZZR')
BEGIN
	INSERT INTO inventory_sources([name], is_active, inventory_source_type) VALUES('BUZZR', 1, 5)
END

IF NOT EXISTS (SELECT * FROM inventory_sources WHERE [name] = 'COZI')
BEGIN
	INSERT INTO inventory_sources([name], is_active, inventory_source_type) VALUES('COZI', 1, 5)
END

IF NOT EXISTS (SELECT * FROM inventory_sources WHERE [name] = 'Escape')
BEGIN
	INSERT INTO inventory_sources([name], is_active, inventory_source_type) VALUES('Escape', 1, 5)
END

IF NOT EXISTS (SELECT * FROM inventory_sources WHERE [name] = 'Grit')
BEGIN
	INSERT INTO inventory_sources([name], is_active, inventory_source_type) VALUES('Grit', 1, 5)
END

IF NOT EXISTS (SELECT * FROM inventory_sources WHERE [name] = 'HITV')
BEGIN
	INSERT INTO inventory_sources([name], is_active, inventory_source_type) VALUES('HITV', 1, 5)
END

IF NOT EXISTS (SELECT * FROM inventory_sources WHERE [name] = 'Laff')
BEGIN
	INSERT INTO inventory_sources([name], is_active, inventory_source_type) VALUES('Laff', 1, 5)
END

IF NOT EXISTS (SELECT * FROM inventory_sources WHERE [name] = 'Me TV')
BEGIN
	INSERT INTO inventory_sources([name], is_active, inventory_source_type) VALUES('Me TV', 1, 5)
END

--make share_projection_book_id, contracted_daypart_id and playback_type nullable
IF EXISTS(SELECT 1 FROM sys.columns WHERE OBJECT_ID = OBJECT_ID('inventory_file_barter_header') AND name = 'share_projection_book_id')
BEGIN
	ALTER TABLE inventory_file_barter_header ALTER COLUMN share_projection_book_id int NULL
END

IF EXISTS(SELECT 1 FROM sys.columns WHERE OBJECT_ID = OBJECT_ID('inventory_file_barter_header') AND name = 'playback_type')
BEGIN
	ALTER TABLE inventory_file_barter_header ALTER COLUMN playback_type int NULL
END

IF EXISTS(SELECT 1 FROM sys.columns WHERE OBJECT_ID = OBJECT_ID('inventory_file_barter_header') AND name = 'contracted_daypart_id')
BEGIN
	ALTER TABLE inventory_file_barter_header ALTER COLUMN contracted_daypart_id int NULL
END
/*************************************** END PRI-6133 *****************************************************/

/*************************************** START PRI-6132 *****************************************************/
--add nti_to_nsi_increase column
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE OBJECT_ID = OBJECT_ID('inventory_file_barter_header') AND name = 'nti_to_nsi_increase')
BEGIN
	ALTER TABLE inventory_file_barter_header ADD nti_to_nsi_increase DECIMAL(18, 10) NULL
END

--add Syndication sources
IF NOT EXISTS (SELECT * FROM inventory_sources WHERE [name] = '20th Century Fox (Twentieth Century)')
BEGIN
	INSERT INTO inventory_sources([name], is_active, inventory_source_type) VALUES('20th Century Fox (Twentieth Century)', 1, 4)
END

IF NOT EXISTS (SELECT * FROM inventory_sources WHERE [name] = 'CBS Synd')
BEGIN
	INSERT INTO inventory_sources([name], is_active, inventory_source_type) VALUES('CBS Synd', 1, 4)
END

IF NOT EXISTS (SELECT * FROM inventory_sources WHERE [name] = 'NBCU Syn')
BEGIN
	INSERT INTO inventory_sources([name], is_active, inventory_source_type) VALUES('NBCU Syn', 1, 4)
END

IF NOT EXISTS (SELECT * FROM inventory_sources WHERE [name] = 'WB Syn')
BEGIN
	INSERT INTO inventory_sources([name], is_active, inventory_source_type) VALUES('WB Syn', 1, 4)
END
/*************************************** END PRI-6132 *****************************************************/

/*************************************** START PRI-5638 *****************************************************/
IF NOT EXISTS(SELECT 1 FROM sys.tables WHERE OBJECT_ID = OBJECT_ID('[dbo].[daypart_codes]'))
BEGIN
	CREATE TABLE [dbo].[daypart_codes](
		[id] [INT] IDENTITY(1,1) NOT NULL,
		[name] [varchar](50) NOT NULL,
		[is_active] [bit] NOT NULL,
	CONSTRAINT [PK_daypart_codes] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]
END

GO

IF NOT EXISTS(SELECT * FROM daypart_codes WHERE [name] = 'EMN')
BEGIN
	INSERT INTO daypart_codes([name], is_active) VALUES('EMN', 1)
END

IF NOT EXISTS(SELECT * FROM daypart_codes WHERE [name] = 'MDN')
BEGIN
	INSERT INTO daypart_codes([name], is_active) VALUES('MDN', 1)
END

IF NOT EXISTS(SELECT * FROM daypart_codes WHERE [name] = 'EN')
BEGIN
	INSERT INTO daypart_codes([name], is_active) VALUES('EN', 1)
END

IF NOT EXISTS(SELECT * FROM daypart_codes WHERE [name] = 'LN')
BEGIN
	INSERT INTO daypart_codes([name], is_active) VALUES('LN', 1)
END

IF NOT EXISTS(SELECT * FROM daypart_codes WHERE [name] = 'ENLN')
BEGIN
	INSERT INTO daypart_codes([name], is_active) VALUES('ENLN', 1)
END

IF NOT EXISTS(SELECT * FROM daypart_codes WHERE [name] = 'EF')
BEGIN
	INSERT INTO daypart_codes([name], is_active) VALUES('EF', 1)
END

IF NOT EXISTS(SELECT * FROM daypart_codes WHERE [name] = 'PA')
BEGIN
	INSERT INTO daypart_codes([name], is_active) VALUES('PA', 1)
END

IF NOT EXISTS(SELECT * FROM daypart_codes WHERE [name] = 'PT')
BEGIN
	INSERT INTO daypart_codes([name], is_active) VALUES('PT', 1)
END

IF NOT EXISTS(SELECT * FROM daypart_codes WHERE [name] = 'LF')
BEGIN
	INSERT INTO daypart_codes([name], is_active) VALUES('LF', 1)
END

IF NOT EXISTS(SELECT * FROM daypart_codes WHERE [name] = 'SYN')
BEGIN
	INSERT INTO daypart_codes([name], is_active) VALUES('SYN', 1)
END

IF NOT EXISTS(SELECT * FROM daypart_codes WHERE [name] = 'OVN')
BEGIN
	INSERT INTO daypart_codes([name], is_active) VALUES('OVN', 1)
END

IF NOT EXISTS(SELECT * FROM daypart_codes WHERE [name] = 'DAY')
BEGIN
	INSERT INTO daypart_codes([name], is_active) VALUES('DAY', 1)
END
/*************************************** END PRI-5638 *****************************************************/

/*************************************** START PRI-5636 *****************************************************/
--add OAndO sources
if not exists (select * from inventory_sources where [name] = 'ABC O&O')
begin
	insert into inventory_sources([name], is_active, inventory_source_type) values('ABC O&O', 1, 3)
end

if not exists (select * from inventory_sources where [name] = 'NBC O&O')
begin
	insert into inventory_sources([name], is_active, inventory_source_type) values('NBC O&O', 1, 3)
end

if not exists (select * from inventory_sources where [name] = 'KATZ')
begin
	insert into inventory_sources([name], is_active, inventory_source_type) values('KATZ', 1, 3)
end

--remove unnecessary 'Assembly' source
if exists (select * from inventory_sources where [name] = 'Assembly')
begin
	delete from inventory_sources where [name] = 'Assembly'
end

--change type of Barter sources
if exists (select * from inventory_sources where [name] = 'TVB')
begin
	update inventory_sources set inventory_source_type = 2 where [name] = 'TVB'
end

if exists (select * from inventory_sources where [name] = 'TTNW')
begin
	update inventory_sources set inventory_source_type = 2 where [name] = 'TTNW'
end

if exists (select * from inventory_sources where [name] = 'CNN')
begin
	update inventory_sources set inventory_source_type = 2 where [name] = 'CNN'
end

if exists (select * from inventory_sources where [name] = 'Sinclair')
begin
	update inventory_sources set inventory_source_type = 2 where [name] = 'Sinclair'
end

if exists (select * from inventory_sources where [name] = 'LilaMax')
begin
	update inventory_sources set inventory_source_type = 2 where [name] = 'LilaMax'
end

if exists (select * from inventory_sources where [name] = 'MLB')
begin
	update inventory_sources set inventory_source_type = 2 where [name] = 'MLB'
end

if exists (select * from inventory_sources where [name] = 'Ference Media')
begin
	update inventory_sources set inventory_source_type = 2 where [name] = 'Ference Media'
end

--make cpm and audience_id nullable
IF EXISTS(SELECT 1 FROM sys.columns WHERE OBJECT_ID = OBJECT_ID('inventory_file_barter_header') AND name = 'cpm')
BEGIN
	ALTER TABLE inventory_file_barter_header ALTER COLUMN cpm money NULL
END

IF EXISTS(SELECT 1 FROM sys.columns WHERE OBJECT_ID = OBJECT_ID('inventory_file_barter_header') AND name = 'audience_id')
BEGIN
	ALTER TABLE inventory_file_barter_header ALTER COLUMN audience_id int NULL
END

/*************************************** END PRI-5636 *****************************************************/

/*************************************** START PRI-5655 ***************************************************/

IF NOT EXISTS(SELECT 1 FROM sys.tables WHERE OBJECT_ID = OBJECT_ID('[dbo].[inventory_file_ratings_jobs]'))
BEGIN
	CREATE TABLE [dbo].inventory_file_ratings_jobs
	(
		[id] [INT] IDENTITY(1,1) NOT NULL,
		[inventory_file_id] [INT] NOT NULL,
		[status] [int] NOT NULL,
		[queued_at] [datetime] NOT NULL,
		[completed_at] [datetime] NULL,
	 CONSTRAINT [PK_inventory_file_ratings_jobs] PRIMARY KEY CLUSTERED 
	 (
		[id] ASC
	 ) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[inventory_file_ratings_jobs] WITH CHECK ADD CONSTRAINT [FK_inventory_file_ratings_jobs_inventory_files] FOREIGN KEY([inventory_file_id])
	REFERENCES [dbo].[inventory_files] ([id])
	ALTER TABLE [dbo].[inventory_file_ratings_jobs] CHECK CONSTRAINT [FK_inventory_file_ratings_jobs_inventory_files]
	CREATE INDEX IX_inventory_file_ratings_jobs_status ON [inventory_file_ratings_jobs] ([status])
END
GO
/*************************************** END PRI-5655 *****************************************************/

/*************************************** START PRI-7081 *****************************************************/
ALTER PROCEDURE [dbo].[usp_GetPostedProposals]
AS
BEGIN

	SELECT
		v.proposal_id as ContractId ,
		v.equivalized as Equivalized,
		p.Name as ContractName,
		(select Max(af.created_date) 
			from proposal_version_details d join proposal_version_detail_quarters q on d.id = q.proposal_version_detail_id
				join proposal_version_detail_quarter_weeks w on w.proposal_version_quarter_id = q.id 
				join affidavit_client_scrubs s on s.proposal_version_detail_quarter_week_id = w.id
				join affidavit_file_details afd on s.affidavit_file_detail_id = afd.id
				join affidavit_files af on af.id = afd.affidavit_file_id
				where d.proposal_version_id = v.id) as UploadDate ,
		(select count(s.id) 
			from proposal_version_details d join proposal_version_detail_quarters q on d.id = q.proposal_version_detail_id
				join proposal_version_detail_quarter_weeks w on w.proposal_version_quarter_id = q.id 
				join affidavit_client_scrubs s on s.proposal_version_detail_quarter_week_id = w.id
				where d.proposal_version_id = v.id and s.status = 2) as SpotsInSpec ,
		(select count(s.id) 
			from proposal_version_details d join proposal_version_detail_quarters q on d.id = q.proposal_version_detail_id
				join proposal_version_detail_quarter_weeks w on w.proposal_version_quarter_id = q.id 
				join affidavit_client_scrubs s on s.proposal_version_detail_quarter_week_id = w.id
				where d.proposal_version_id = v.id and s.status = 1)as SpotsOutOfSpec ,
		p.advertiser_id as AdvertiserId,
		v.guaranteed_audience_id as GuaranteedAudienceId,
		v.post_type as PostType,
     		(select isnull(sum(q.impressions_goal),0)  --Added isnull 2019-04-08
			from proposal_version_details d join proposal_version_detail_quarters q on d.id = q.proposal_version_detail_id
				where d.proposal_version_id = v.id) as PrimaryAudienceBookedImpressions 
	from proposal_versions v
		join proposals p on p.id = v.proposal_id
	where v.status = 3
END
GO
/*************************************** END PRI-7081 *****************************************************/

/*************************************** START PRI-7386 *****************************************************/

IF (SELECT name FROM inventory_sources
    WHERE id = 4) = 'TTNW'
BEGIN
	UPDATE inventory_sources
	SET name = 'TTWN'
	WHERE id = 4
END

/*************************************** END PRI-7386 *******************************************************/

/*************************************** END UPDATE SCRIPT *******************************************************/

-- Update the Schema Version of the database to the current release version
UPDATE system_component_parameters 
SET parameter_value = '19.05.1' -- Current release version
WHERE parameter_key = 'SchemaVersion'
GO

print 'Updated schema Version'

IF(XACT_STATE() = 1)
BEGIN 
	
	IF EXISTS (SELECT TOP 1 * 
		FROM #previous_version 
		WHERE [version] = '19.04.1' -- Previous release version
		OR [version] = '19.05.1') -- Current release version
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