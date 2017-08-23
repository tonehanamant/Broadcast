CREATE PROCEDURE [dbo].[usp_images_select]
(
	@id Int
)
AS
SELECT
	id,
	file_name,
	file_size,
	mime_type,
	image_data,
	height,
	width,
	modified_on,
	modified_by
FROM
	images (NOLOCK)
WHERE
	id = @id
