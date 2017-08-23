
-- =============================================
-- Author:		CRUD Creator
-- Create date: 11/29/2016 02:29:17 PM
-- Description:	Auto-generated method to update a zone_networks record.
-- =============================================
CREATE PROCEDURE usp_zone_networks_update
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
	UPDATE
		[dbo].[zone_networks]
	SET
		[source]=@source,
		[trafficable]=@trafficable,
		[primary]=@primary,
		[subscribers]=@subscribers,
		[effective_date]=@effective_date,
		[feed_type]=@feed_type
	WHERE
		[zone_id]=@zone_id
		AND [network_id]=@network_id
END
