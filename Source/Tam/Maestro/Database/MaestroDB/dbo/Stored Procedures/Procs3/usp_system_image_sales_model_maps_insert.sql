
CREATE PROCEDURE [dbo].[usp_system_image_sales_model_maps_insert]
(
	@id		int		OUTPUT,
	@system_id		Int,
	@image_id		Int,
	@sales_model_id		Int,
	@map_set		VarChar(63),
	@effective_date		DateTime
)
AS
INSERT INTO system_image_sales_model_maps
(
	system_id,
	image_id,
	sales_model_id,
	map_set,
	effective_date
)
VALUES
(
	@system_id,
	@image_id,
	@sales_model_id,
	@map_set,
	@effective_date
)

SELECT
	@id = SCOPE_IDENTITY()


