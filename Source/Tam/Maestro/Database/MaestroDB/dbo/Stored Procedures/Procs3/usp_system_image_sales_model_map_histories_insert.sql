
CREATE PROCEDURE [dbo].[usp_system_image_sales_model_map_histories_insert]
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
INSERT INTO system_image_sales_model_map_histories
(
	system_image_sales_model_map_id,
	start_date,
	system_id,
	image_id,
	sales_model_id,
	map_set,
	end_date
)
VALUES
(
	@system_image_sales_model_map_id,
	@start_date,
	@system_id,
	@image_id,
	@sales_model_id,
	@map_set,
	@end_date
)


