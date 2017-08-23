CREATE PROCEDURE usp_countries_update
(
	@id		Int,
	@code		VarChar(15),
	@name		VarChar(127)
)
AS
UPDATE countries SET
	code = @code,
	name = @name
WHERE
	id = @id

