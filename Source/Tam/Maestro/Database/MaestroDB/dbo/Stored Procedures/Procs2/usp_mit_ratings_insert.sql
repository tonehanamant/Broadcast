-- =============================================
-- Author:		CRUD Creator
-- Create date: 10/22/2013 10:06:58 AM
-- Description:	Auto-generated method to insert a mit_ratings record.
-- =============================================
CREATE PROCEDURE [dbo].[usp_mit_ratings_insert]
	@media_month_id INT,
	@rating_category_id INT,
	@nielsen_network_id INT,
	@rating_date DATE,
	@start_time INT,
	@end_time INT,
	@feed_type VARCHAR(15),
	@id INT OUTPUT
AS
BEGIN
	INSERT INTO [dbo].[mit_ratings]
	(
		[media_month_id],
		[rating_category_id],
		[nielsen_network_id],
		[rating_date],
		[start_time],
		[end_time],
		[feed_type]
	)
	VALUES
	(
		@media_month_id,
		@rating_category_id,
		@nielsen_network_id,
		@rating_date,
		@start_time,
		@end_time,
		@feed_type
	)

	SELECT
		@id = SCOPE_IDENTITY()
END
