CREATE PROCEDURE usp_nielsen_suggested_rates_insert
(
	@id		Int		OUTPUT,
	@year		Int,
	@quarter		Int,
	@media_month_id		Int
)
AS
INSERT INTO nielsen_suggested_rates
(
	year,
	quarter,
	media_month_id
)
VALUES
(
	@year,
	@quarter,
	@media_month_id
)

SELECT
	@id = SCOPE_IDENTITY()

