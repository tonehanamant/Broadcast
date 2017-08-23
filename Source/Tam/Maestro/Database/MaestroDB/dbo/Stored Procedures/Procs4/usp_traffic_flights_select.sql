CREATE PROCEDURE usp_traffic_flights_select
(
	@traffic_id		Int,
	@start_date		DateTime
)
AS
SELECT
	*
FROM
	traffic_flights WITH(NOLOCK)
WHERE
	traffic_id=@traffic_id
	AND
	start_date=@start_date

