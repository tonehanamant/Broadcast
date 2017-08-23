CREATE PROCEDURE [dbo].[usp_materials_delete]
(
	@id Int
)
AS
DELETE FROM dbo.materials WHERE id=@id
