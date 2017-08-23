CREATE PROCEDURE [dbo].[usp_images_update]
(
	@id		Int,
	@file_name		VarChar(255),
	@file_size		Int,
	@mime_type		VarChar(31),
	@image_data		VARBINARY(MAX),
	@height		Int,
	@width		Int,
	@modified_on		DateTime,
	@modified_by		Int
)
AS
UPDATE images SET
	file_name = @file_name,
	file_size = @file_size,
	mime_type = @mime_type,
	image_data = @image_data,
	height = @height,
	width = @width,
	modified_on = @modified_on,
	modified_by = @modified_by
WHERE
	id = @id

