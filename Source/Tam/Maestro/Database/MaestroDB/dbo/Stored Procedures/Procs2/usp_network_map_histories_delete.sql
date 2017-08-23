CREATE PROCEDURE usp_network_map_histories_delete
(
	@network_map_id		Int,
	@start_date		DateTime)
AS
DELETE FROM
	network_map_histories
WHERE
	network_map_id = @network_map_id
 AND
	start_date = @start_date
