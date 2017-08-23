CREATE PROCEDURE usp_system_group_histories_insert
(
	@system_group_id		Int,
	@start_date		DateTime,
	@name		VarChar(63),
	@active		Bit,
	@flag		TinyInt,
	@end_date		DateTime
)
AS
INSERT INTO system_group_histories
(
	system_group_id,
	start_date,
	name,
	active,
	flag,
	end_date
)
VALUES
(
	@system_group_id,
	@start_date,
	@name,
	@active,
	@flag,
	@end_date
)

