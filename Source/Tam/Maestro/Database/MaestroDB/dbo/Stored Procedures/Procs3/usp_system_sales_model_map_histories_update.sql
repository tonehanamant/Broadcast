
CREATE PROCEDURE [dbo].[usp_system_sales_model_map_histories_update]
(
	@system_sales_model_map_id		Int,
	@start_date		DateTime,
	@system_id		Int,
	@sales_model_id		Int,
	@map_set		VarChar(63),
	@map_value		VARCHAR(MAX),
	@end_date		DateTime
)
AS
UPDATE system_sales_model_map_histories SET
	system_id = @system_id,
	sales_model_id = @sales_model_id,
	map_set = @map_set,
	map_value = @map_value,
	end_date = @end_date
WHERE
	system_sales_model_map_id = @system_sales_model_map_id AND
	start_date = @start_date

