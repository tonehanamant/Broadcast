
CREATE PROCEDURE [dbo].[usp_system_image_sales_model_map_histories_delete]
(
	@system_image_sales_model_map_id		Int,
	@start_date		DateTime)
AS
DELETE FROM
	system_image_sales_model_map_histories
WHERE
	system_image_sales_model_map_id = @system_image_sales_model_map_id
 AND
	start_date = @start_date

