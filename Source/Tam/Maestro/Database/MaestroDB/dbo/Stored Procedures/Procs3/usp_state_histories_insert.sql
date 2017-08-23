CREATE PROCEDURE usp_state_histories_insert
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
INSERT INTO state_histories
(
	state_id,
	start_date,
	code,
	name,
	active,
	flag,
	end_date
)
VALUES
(
	@state_id,
	@start_date,
	@code,
	@name,
	@active,
	@flag,
	@end_date
)

