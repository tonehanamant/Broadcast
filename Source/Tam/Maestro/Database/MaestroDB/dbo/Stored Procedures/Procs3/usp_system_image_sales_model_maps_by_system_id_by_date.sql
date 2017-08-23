

Create Proc [dbo].[usp_system_image_sales_model_maps_by_system_id_by_date]
	 @system_id int
	,@effective_date datetime
AS
BEGIN
SELECT 
	 [system_image_sales_model_map_id]
	,[system_id]
	,[image_id]
	,[sales_model_id]
	,[map_set]
	,[effective_date] = [start_date]
FROM 
	[uvw_system_image_sales_model_map_universe] WITH(NOLOCK)
WHERE
	[system_id] = @system_id
	AND
		([uvw_system_image_sales_model_map_universe].start_date<=@effective_date 
		AND 
			([uvw_system_image_sales_model_map_universe].end_date>=@effective_date 
			OR [uvw_system_image_sales_model_map_universe].end_date IS NULL))
END
