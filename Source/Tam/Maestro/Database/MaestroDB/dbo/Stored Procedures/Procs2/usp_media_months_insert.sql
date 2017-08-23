CREATE PROCEDURE usp_media_months_insert
(
	@id		Int		OUTPUT,
	@year		Int,
	@month		Int,
	@media_month		VarChar(15),
	@start_date		DateTime,
	@end_date		DateTime
)
AS
INSERT INTO media_months
(
	year,
	month,
	media_month,
	start_date,
	end_date
)
VALUES
(
	@year,
	@month,
	@media_month,
	@start_date,
	@end_date
)

SELECT
	@id = SCOPE_IDENTITY()

