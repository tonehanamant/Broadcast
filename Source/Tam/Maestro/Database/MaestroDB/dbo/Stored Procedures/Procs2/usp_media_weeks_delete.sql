CREATE PROCEDURE usp_media_weeks_delete
(
	@id Int
)
AS
DELETE FROM media_weeks WHERE id=@id
