CREATE PROCEDURE usp_media_weeks_update
(
	@id		Int,
	@media_month_id		Int,
	@week_number		Int,
	@start_date		DateTime,
	@end_date		DateTime
)
AS
UPDATE media_weeks SET
	media_month_id = @media_month_id,
	week_number = @week_number,
	start_date = @start_date,
	end_date = @end_date
WHERE
	id = @id

