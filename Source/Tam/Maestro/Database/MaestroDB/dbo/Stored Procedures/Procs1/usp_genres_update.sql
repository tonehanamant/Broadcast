CREATE PROCEDURE usp_genres_update
(
	@id		Int,
	@name		VarChar(63)
)
AS
BEGIN
UPDATE dbo.genres SET
	name = @name
WHERE
	id = @id
END
