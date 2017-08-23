CREATE PROCEDURE usp_system_groups_update
(
	@id		Int,
	@name		VarChar(63),
	@active		Bit,
	@flag		TinyInt,
	@effective_date		DateTime
)
AS
UPDATE system_groups SET
	name = @name,
	active = @active,
	flag = @flag,
	effective_date = @effective_date
WHERE
	id = @id

