CREATE PROCEDURE usp_traffic_flights_update
(
	@traffic_id		Int,
	@start_date		DateTime,
	@end_date		DateTime,
	@selected		Bit
)
AS
UPDATE traffic_flights SET
	end_date = @end_date,
	selected = @selected
WHERE
	traffic_id = @traffic_id AND
	start_date = @start_date
