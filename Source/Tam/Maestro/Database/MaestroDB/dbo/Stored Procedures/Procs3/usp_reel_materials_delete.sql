CREATE PROCEDURE usp_reel_materials_delete
(
	@id Int
)
AS
DELETE FROM reel_materials WHERE id=@id
