CREATE PROCEDURE usp_cmw_traffic_flights_update
(
	@cmw_traffic_id		Int,
	@start_date		DateTime,
	@end_date		DateTime,
	@selected		Bit
)
AS
UPDATE cmw_traffic_flights SET
	end_date = @end_date,
	selected = @selected
WHERE
	cmw_traffic_id = @cmw_traffic_id AND
	start_date = @start_date
