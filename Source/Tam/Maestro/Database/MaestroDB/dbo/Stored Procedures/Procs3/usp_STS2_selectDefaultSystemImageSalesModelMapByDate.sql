
-- =============================================
-- Author:		jcarsley
-- Create date: 11/17/2010
-- Description:	Gets the default image logo for a sales model, and map_set - by date
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectDefaultSystemImageSalesModelMapByDate]
	 @sales_model_id int 
	,@map_set varchar(63)
	,@effective_date datetime
AS
BEGIN
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT 
		id = system_image_sales_model_map_id,
		system_id,
		image_id,
		sales_model_id,
		map_set,
		effective_date = start_date
   from
		uvw_system_image_sales_model_map_universe WITH(NOLOCK)
	where
		system_id = null
		and sales_model_id = @sales_model_id
		and map_set = @map_set
		AND
			(uvw_system_image_sales_model_map_universe.start_date<=@effective_date 
			AND 
				(uvw_system_image_sales_model_map_universe.end_date>=@effective_date 
				OR uvw_system_image_sales_model_map_universe.end_date IS NULL))
	
END

