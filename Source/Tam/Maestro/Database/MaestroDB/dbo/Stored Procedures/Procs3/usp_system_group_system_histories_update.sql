CREATE PROCEDURE usp_system_group_system_histories_update
(
	@system_group_id		Int,
	@system_id		Int,
	@start_date		DateTime,
	@end_date		DateTime
)
AS
UPDATE system_group_system_histories SET
	end_date = @end_date
WHERE
	system_group_id = @system_group_id AND
	system_id = @system_id AND
	start_date = @start_date
