CREATE PROCEDURE usp_media_months_delete
(
	@id Int
)
AS
DELETE FROM media_months WHERE id=@id
