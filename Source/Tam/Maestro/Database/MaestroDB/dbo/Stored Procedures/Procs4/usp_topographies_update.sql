CREATE PROCEDURE usp_topographies_update
(
	@id		Int,
	@code		VarChar(15),
	@name		VarChar(63),
	@topography_type		TinyInt
)
AS
UPDATE topographies SET
	code = @code,
	name = @name,
	topography_type = @topography_type
WHERE
	id = @id

