CREATE PROCEDURE usp_company_types_update
(
	@id		Int,
	@name		VarChar(20),
	@code		VarChar(3)
)
AS
UPDATE company_types SET
	name = @name,
	code = @code
WHERE
	id = @id

