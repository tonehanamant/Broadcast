CREATE PROCEDURE usp_days_insert
(
	@id		Int		OUTPUT,
	@code_1		Char(10),
	@code_2		Char(10),
	@code_3		Char(10),
	@name		VarChar(15),
	@ordinal		Int
)
AS
INSERT INTO days
(
	code_1,
	code_2,
	code_3,
	name,
	ordinal
)
VALUES
(
	@code_1,
	@code_2,
	@code_3,
	@name,
	@ordinal
)

SELECT
	@id = SCOPE_IDENTITY()

