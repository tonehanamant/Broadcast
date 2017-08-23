CREATE PROCEDURE usp_network_map_histories_select
(
	@network_map_id		Int,
	@start_date		DateTime
)
AS
SELECT
	*
FROM
	network_map_histories WITH(NOLOCK)
WHERE
	network_map_id=@network_map_id
	AND
	start_date=@start_date

