
-- =============================================
-- Author:		CRUD Creator
-- Create date: 11/29/2016 02:29:16 PM
-- Description:	Auto-generated method to insert a zone_networks record.
-- =============================================
CREATE PROCEDURE usp_zone_networks_insert
	@zone_id INT,
	@network_id INT,
	@source VARCHAR(15),
	@trafficable BIT,
	@primary BIT,
	@subscribers INT,
	@effective_date DATETIME,
	@feed_type TINYINT
AS
BEGIN
	INSERT INTO [dbo].[zone_networks]
	(
		[zone_id],
		[network_id],
		[source],
		[trafficable],
		[primary],
		[subscribers],
		[effective_date],
		[feed_type]
	)
	VALUES
	(
		@zone_id,
		@network_id,
		@source,
		@trafficable,
		@primary,
		@subscribers,
		@effective_date,
		@feed_type
	)
END
