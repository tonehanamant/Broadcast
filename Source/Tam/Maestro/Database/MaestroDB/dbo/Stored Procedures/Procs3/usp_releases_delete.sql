CREATE PROCEDURE usp_releases_delete
(
	@id Int
)
AS
DELETE FROM releases WHERE id=@id
