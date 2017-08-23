
CREATE PROCEDURE [dbo].[usp_system_image_sales_model_map_histories_update]
(
	@system_image_sales_model_map_id		Int,
	@start_date		DateTime,
	@system_id		Int,
	@image_id		Int,
	@sales_model_id		Int,
	@map_set		VarChar(63),
	@end_date		DateTime
)
AS
UPDATE system_image_sales_model_map_histories SET
	system_id = @system_id,
	image_id = @image_id,
	sales_model_id = @sales_model_id,
	map_set = @map_set,
	end_date = @end_date
WHERE
	system_image_sales_model_map_id = @system_image_sales_model_map_id AND
	start_date = @start_date

