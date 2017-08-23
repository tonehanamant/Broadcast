
-- =============================================
-- Author:		CRUD Creator
-- Create date: 11/29/2016 02:29:16 PM
-- Description:	Auto-generated method to update a zone_network_histories record.
-- =============================================
CREATE PROCEDURE usp_zone_network_histories_update
	@zone_id INT,
	@network_id INT,
	@start_date DATETIME,
	@source VARCHAR(15),
	@trafficable BIT,
	@primary BIT,
	@subscribers INT,
	@end_date DATETIME,
	@feed_type TINYINT
AS
BEGIN
	UPDATE
		[dbo].[zone_network_histories]
	SET
		[source]=@source,
		[trafficable]=@trafficable,
		[primary]=@primary,
		[subscribers]=@subscribers,
		[end_date]=@end_date,
		[feed_type]=@feed_type
	WHERE
		[zone_id]=@zone_id
		AND [network_id]=@network_id
		AND [start_date]=@start_date
END
