CREATE PROCEDURE [dbo].[usp_images_insert]
(
	@id		int		OUTPUT,
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
INSERT INTO images
(
	file_name,
	file_size,
	mime_type,
	image_data,
	height,
	width,
	modified_on,
	modified_by
)
VALUES
(
	@file_name,
	@file_size,
	@mime_type,
	@image_data,
	@height,
	@width,
	@modified_on,
	@modified_by
)

SELECT
	@id = SCOPE_IDENTITY()

