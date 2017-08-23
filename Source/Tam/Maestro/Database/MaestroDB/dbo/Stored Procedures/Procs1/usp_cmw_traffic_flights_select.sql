CREATE PROCEDURE usp_cmw_traffic_flights_select
(
	@cmw_traffic_id		Int,
	@start_date		DateTime
)
AS
SELECT
	*
FROM
	cmw_traffic_flights WITH(NOLOCK)
WHERE
	cmw_traffic_id=@cmw_traffic_id
	AND
	start_date=@start_date

