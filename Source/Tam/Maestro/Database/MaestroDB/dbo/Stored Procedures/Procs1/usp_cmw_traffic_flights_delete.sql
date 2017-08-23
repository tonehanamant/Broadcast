CREATE PROCEDURE usp_cmw_traffic_flights_delete
(
	@cmw_traffic_id		Int,
	@start_date		DateTime)
AS
DELETE FROM
	cmw_traffic_flights
WHERE
	cmw_traffic_id = @cmw_traffic_id
 AND
	start_date = @start_date
