
CREATE PROCEDURE [dbo].[usp_system_sales_model_maps_update]
(
	@id		Int,
	@system_id		Int,
	@sales_model_id		Int,
	@map_set		VarChar(63),
	@map_value		VARCHAR(MAX),
	@effective_date		DateTime
)
AS
UPDATE system_sales_model_maps SET
	system_id = @system_id,
	sales_model_id = @sales_model_id,
	map_set = @map_set,
	map_value = @map_value,
	effective_date = @effective_date
WHERE
	id = @id


