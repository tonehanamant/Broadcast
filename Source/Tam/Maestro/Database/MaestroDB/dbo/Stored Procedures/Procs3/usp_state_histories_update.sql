CREATE PROCEDURE usp_state_histories_update
(
	@state_id		Int,
	@start_date		DateTime,
	@code		VarChar(15),
	@name		VarChar(63),
	@active		Bit,
	@flag		TinyInt,
	@end_date		DateTime
)
AS
UPDATE state_histories SET
	code = @code,
	name = @name,
	active = @active,
	flag = @flag,
	end_date = @end_date
WHERE
	state_id = @state_id AND
	start_date = @start_date
