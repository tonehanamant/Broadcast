CREATE PROCEDURE usp_cmw_materials_delete
(
	@id Int
)
AS
DELETE FROM cmw_materials WHERE id=@id
