CREATE PROCEDURE usp_traffic_detail_weeks_update
(
	@id		Int,
	@traffic_detail_id		Int,
	@start_date		DateTime,
	@end_date		DateTime,
	@suspended		Bit
)
AS

UPDATE traffic_detail_weeks SET
	traffic_detail_id = @traffic_detail_id,
	start_date = @start_date,
	end_date = @end_date,
	suspended = @suspended
WHERE
	id = @id

