CREATE PROCEDURE usp_traffic_flights_delete
(
	@traffic_id		Int,
	@start_date		DateTime)
AS
DELETE FROM
	traffic_flights
WHERE
	traffic_id = @traffic_id
 AND
	start_date = @start_date
