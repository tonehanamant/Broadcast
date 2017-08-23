CREATE PROCEDURE usp_media_months_update
(
	@id		Int,
	@year		Int,
	@month		Int,
	@media_month		VarChar(15),
	@start_date		DateTime,
	@end_date		DateTime
)
AS
UPDATE media_months SET
	year = @year,
	month = @month,
	media_month = @media_month,
	start_date = @start_date,
	end_date = @end_date
WHERE
	id = @id

