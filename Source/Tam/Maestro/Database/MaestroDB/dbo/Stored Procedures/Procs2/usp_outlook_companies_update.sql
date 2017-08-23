CREATE PROCEDURE usp_outlook_companies_update
(
	@id		Int,
	@name		VarChar(127)
)
AS
UPDATE outlook_companies SET
	name = @name
WHERE
	id = @id

