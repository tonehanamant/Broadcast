CREATE PROCEDURE usp_system_group_system_histories_insert
(
	@system_group_id		Int,
	@system_id		Int,
	@start_date		DateTime,
	@end_date		DateTime
)
AS
INSERT INTO system_group_system_histories
(
	system_group_id,
	system_id,
	start_date,
	end_date
)
VALUES
(
	@system_group_id,
	@system_id,
	@start_date,
	@end_date
)

