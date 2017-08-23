CREATE PROCEDURE usp_nielsen_suggested_rates_update
(
	@id		Int,
	@year		Int,
	@quarter		Int,
	@media_month_id		Int
)
AS
UPDATE nielsen_suggested_rates SET
	year = @year,
	quarter = @quarter,
	media_month_id = @media_month_id
WHERE
	id = @id

