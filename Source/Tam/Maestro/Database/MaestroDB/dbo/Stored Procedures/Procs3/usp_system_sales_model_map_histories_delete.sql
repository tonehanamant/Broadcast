
CREATE PROCEDURE [dbo].[usp_system_sales_model_map_histories_delete]
(
	@system_sales_model_map_id		Int,
	@start_date		DateTime)
AS
DELETE FROM
	system_sales_model_map_histories
WHERE
	system_sales_model_map_id = @system_sales_model_map_id
 AND
	start_date = @start_date

