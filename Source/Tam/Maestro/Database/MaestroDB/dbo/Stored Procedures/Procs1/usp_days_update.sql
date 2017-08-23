CREATE PROCEDURE usp_days_update
(
	@id		Int,
	@code_1		Char(10),
	@code_2		Char(10),
	@code_3		Char(10),
	@name		VarChar(15),
	@ordinal		Int
)
AS
UPDATE days SET
	code_1 = @code_1,
	code_2 = @code_2,
	code_3 = @code_3,
	name = @name,
	ordinal = @ordinal
WHERE
	id = @id

