CREATE PROCEDURE usp_media_weeks_insert
(
	@id		Int		OUTPUT,
	@media_month_id		Int,
	@week_number		Int,
	@start_date		DateTime,
	@end_date		DateTime
)
AS
INSERT INTO media_weeks
(
	media_month_id,
	week_number,
	start_date,
	end_date
)
VALUES
(
	@media_month_id,
	@week_number,
	@start_date,
	@end_date
)

SELECT
	@id = SCOPE_IDENTITY()

