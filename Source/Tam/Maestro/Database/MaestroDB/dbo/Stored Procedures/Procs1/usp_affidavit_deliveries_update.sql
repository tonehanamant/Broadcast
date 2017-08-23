CREATE PROCEDURE [dbo].[usp_affidavit_deliveries_update]
(
	@affidavit_id		BigInt,
	@media_month_id		Int,
	@audience_id		Int,
	@rating_source_id		TinyInt,
	@audience_usage		Float,
	@universe		Float,
	@regional_usage Float,
	@regional_rating Float
)
AS
UPDATE affidavit_deliveries SET
	audience_usage = @audience_usage,
	universe = @universe,
	regional_usage = @regional_usage,
	regional_rating = @regional_rating
WHERE
	affidavit_id = @affidavit_id AND
	media_month_id = @media_month_id AND
	audience_id = @audience_id AND
	rating_source_id = @rating_source_id
