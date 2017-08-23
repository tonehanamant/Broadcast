
CREATE PROCEDURE [dbo].[usp_system_sales_model_map_histories_select_all]
AS
SELECT
	system_sales_model_map_id,
	start_date,
	system_id,
	sales_model_id,
	map_set,
	map_value,
	end_date
FROM
	system_sales_model_map_histories WITH(NOLOCK)

