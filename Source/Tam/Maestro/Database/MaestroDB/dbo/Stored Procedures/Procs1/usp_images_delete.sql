CREATE PROCEDURE [dbo].[usp_images_delete]
(
	@id Int)
AS
DELETE FROM images WHERE id=@id
