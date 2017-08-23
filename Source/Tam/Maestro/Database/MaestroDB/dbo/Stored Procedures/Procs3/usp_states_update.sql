CREATE PROCEDURE usp_states_update
(
	@id		Int,
	@code		VarChar(15),
	@name		VarChar(63),
	@active		Bit,
	@flag		TinyInt,
	@effective_date		DateTime
)
AS
UPDATE states SET
	code = @code,
	name = @name,
	active = @active,
	flag = @flag,
	effective_date = @effective_date
WHERE
	id = @id

