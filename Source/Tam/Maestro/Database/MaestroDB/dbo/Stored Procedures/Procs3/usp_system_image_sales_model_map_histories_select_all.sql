
CREATE PROCEDURE [dbo].[usp_system_image_sales_model_map_histories_select_all]
AS
SELECT
	system_image_sales_model_map_id,
	start_date,
	system_id,
	image_id,
	sales_model_id,
	map_set,
	end_date
FROM
	system_image_sales_model_map_histories WITH(NOLOCK)

