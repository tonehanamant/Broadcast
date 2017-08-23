
-- =============================================
-- Author:		CRUD Creator
-- Create date: 11/29/2016 02:29:16 PM
-- Description:	Auto-generated method to insert a zone_network_histories record.
-- =============================================
CREATE PROCEDURE usp_zone_network_histories_insert
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
	INSERT INTO [dbo].[zone_network_histories]
	(
		[zone_id],
		[network_id],
		[start_date],
		[source],
		[trafficable],
		[primary],
		[subscribers],
		[end_date],
		[feed_type]
	)
	VALUES
	(
		@zone_id,
		@network_id,
		@start_date,
		@source,
		@trafficable,
		@primary,
		@subscribers,
		@end_date,
		@feed_type
	)
END
