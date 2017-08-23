CREATE PROCEDURE usp_system_custom_traffic_histories_delete
(
	@system_id		Int,
	@start_date		DateTime)
AS
DELETE FROM
	system_custom_traffic_histories
WHERE
	system_id = @system_id
 AND
	start_date = @start_date
