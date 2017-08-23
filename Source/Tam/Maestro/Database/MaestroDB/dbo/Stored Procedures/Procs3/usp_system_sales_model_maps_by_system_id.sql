


CREATE Proc [dbo].[usp_system_sales_model_maps_by_system_id]
	@system_id int
AS
BEGIN
SELECT 
	 [id]
	,[system_id]
	,[sales_model_id]
	,[map_set]
	,[map_value]
	,[effective_date]
FROM 
	[dbo].[system_sales_model_maps] with(NOLOCK)
WHERE
	[system_id] = @system_id
END
