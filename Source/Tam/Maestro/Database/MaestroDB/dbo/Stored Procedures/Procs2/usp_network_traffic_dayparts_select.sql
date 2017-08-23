CREATE PROCEDURE usp_network_traffic_dayparts_select
(
	@nielsen_network_id		Int,
	@daypart_id		Int
)
AS
SELECT
	*
FROM
	network_traffic_dayparts WITH(NOLOCK)
WHERE
	nielsen_network_id=@nielsen_network_id
	AND
	daypart_id=@daypart_id

