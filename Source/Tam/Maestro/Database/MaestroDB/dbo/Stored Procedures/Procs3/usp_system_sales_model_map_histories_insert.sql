
CREATE PROCEDURE [dbo].[usp_system_sales_model_map_histories_insert]
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
INSERT INTO system_sales_model_map_histories
(
	system_sales_model_map_id,
	start_date,
	system_id,
	sales_model_id,
	map_set,
	map_value,
	end_date
)
VALUES
(
	@system_sales_model_map_id,
	@start_date,
	@system_id,
	@sales_model_id,
	@map_set,
	@map_value,
	@end_date
)


