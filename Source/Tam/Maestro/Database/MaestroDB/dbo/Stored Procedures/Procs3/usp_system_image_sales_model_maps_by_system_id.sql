

CREATE Proc [dbo].[usp_system_image_sales_model_maps_by_system_id]
	@system_id int
AS
BEGIN
SELECT 
	 [id]
	,[system_id]
	,[image_id]
	,[sales_model_id]
	,[map_set]
	,[effective_date]
FROM 
	[dbo].[system_image_sales_model_maps] with(NOLOCK)
WHERE
	[system_id] = @system_id
END
