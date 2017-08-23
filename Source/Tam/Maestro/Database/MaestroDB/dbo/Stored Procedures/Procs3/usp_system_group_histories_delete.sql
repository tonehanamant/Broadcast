CREATE PROCEDURE usp_system_group_histories_delete
(
	@system_group_id		Int,
	@start_date		DateTime)
AS
DELETE FROM
	system_group_histories
WHERE
	system_group_id = @system_group_id
 AND
	start_date = @start_date
