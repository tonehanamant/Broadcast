CREATE PROCEDURE usp_genres_delete
(
	@id		Int)
AS
BEGIN
DELETE FROM
	dbo.genres
WHERE
	id = @id
END
