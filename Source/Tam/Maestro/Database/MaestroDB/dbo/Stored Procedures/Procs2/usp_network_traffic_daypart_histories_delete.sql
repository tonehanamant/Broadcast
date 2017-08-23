CREATE PROCEDURE usp_network_traffic_daypart_histories_delete
(
	@nielsen_network_id		Int,
	@daypart_id		Int,
	@start_date		DateTime)
AS
DELETE FROM
	network_traffic_daypart_histories
WHERE
	nielsen_network_id = @nielsen_network_id
 AND
	daypart_id = @daypart_id
 AND
	start_date = @start_date
