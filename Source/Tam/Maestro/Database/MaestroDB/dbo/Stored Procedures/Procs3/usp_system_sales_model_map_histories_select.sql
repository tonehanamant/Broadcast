
CREATE PROCEDURE [dbo].[usp_system_sales_model_map_histories_select]
(
	@system_sales_model_map_id		Int,
	@start_date		DateTime
)
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
WHERE
	system_sales_model_map_id=@system_sales_model_map_id
	AND
	start_date=@start_date


