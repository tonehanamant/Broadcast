CREATE PROCEDURE [dbo].[usp_affidavit_deliveries_insert]
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
INSERT INTO affidavit_deliveries
(
	affidavit_id,
	media_month_id,
	audience_id,
	rating_source_id,
	audience_usage,
	universe,
	regional_usage,
	regional_rating
)
VALUES
(
	@affidavit_id,
	@media_month_id,
	@audience_id,
	@rating_source_id,
	@audience_usage,
	@universe,
	@regional_usage,
	@regional_rating
)
