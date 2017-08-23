CREATE PROCEDURE [dbo].[usp_mit_universes_update]
(
	@media_month_id		Int,
	@rating_category_id		Int,
	@nielsen_network_id		Int,
	@start_date		Date,
	@end_date		Date,
	@id		Int
)
AS
UPDATE dbo.mit_universes SET
	media_month_id = @media_month_id,
	rating_category_id = @rating_category_id,
	nielsen_network_id = @nielsen_network_id,
	start_date = @start_date,
	end_date = @end_date
WHERE
	id = @id
