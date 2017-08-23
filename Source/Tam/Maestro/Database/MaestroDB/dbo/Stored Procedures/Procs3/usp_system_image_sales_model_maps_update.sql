
CREATE PROCEDURE [dbo].[usp_system_image_sales_model_maps_update]
(
	@id		Int,
	@system_id		Int,
	@image_id		Int,
	@sales_model_id		Int,
	@map_set		VarChar(63),
	@effective_date		DateTime
)
AS
UPDATE system_image_sales_model_maps SET
	system_id = @system_id,
	image_id = @image_id,
	sales_model_id = @sales_model_id,
	map_set = @map_set,
	effective_date = @effective_date
WHERE
	id = @id


