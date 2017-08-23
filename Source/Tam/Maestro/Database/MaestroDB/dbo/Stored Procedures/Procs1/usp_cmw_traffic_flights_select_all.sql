CREATE PROCEDURE usp_cmw_traffic_flights_select_all
AS
SELECT
	*
FROM
	cmw_traffic_flights WITH(NOLOCK)
