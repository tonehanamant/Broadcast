CREATE PROCEDURE usp_system_group_system_histories_delete
(
	@system_group_id		Int,
	@system_id		Int,
	@start_date		DateTime)
AS
DELETE FROM
	system_group_system_histories
WHERE
	system_group_id = @system_group_id
 AND
	system_id = @system_id
 AND
	start_date = @start_date
