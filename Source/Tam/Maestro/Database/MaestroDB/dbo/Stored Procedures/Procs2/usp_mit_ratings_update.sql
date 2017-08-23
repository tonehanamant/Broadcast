CREATE PROCEDURE [dbo].[usp_mit_ratings_update]
(
	@media_month_id		Int,
	@rating_category_id		Int,
	@nielsen_network_id		Int,
	@rating_date		Date,
	@start_time		Int,
	@end_time		Int,
	@feed_type		VarChar(15),
	@id		Int
)
AS
UPDATE dbo.mit_ratings SET
	media_month_id = @media_month_id,
	rating_category_id = @rating_category_id,
	nielsen_network_id = @nielsen_network_id,
	rating_date = @rating_date,
	start_time = @start_time,
	end_time = @end_time,
	feed_type = @feed_type
WHERE
	id = @id
