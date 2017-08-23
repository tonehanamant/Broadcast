CREATE PROCEDURE usp_system_group_histories_update
(
	@system_group_id		Int,
	@start_date		DateTime,
	@name		VarChar(63),
	@active		Bit,
	@flag		TinyInt,
	@end_date		DateTime
)
AS
UPDATE system_group_histories SET
	name = @name,
	active = @active,
	flag = @flag,
	end_date = @end_date
WHERE
	system_group_id = @system_group_id AND
	start_date = @start_date
