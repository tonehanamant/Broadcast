CREATE PROCEDURE usp_traffic_flights_select_all
AS
SELECT
	*
FROM
	traffic_flights WITH(NOLOCK)
