
-- =============================================
-- Author:		jcarsley
-- Create date: 10/26/2010
-- Description:	Gets the disclaimer text for a sytem, sales model, and map_set
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectDisclaimerTextByDate]
	 @system_id int
	,@sales_model_id int 
	,@map_set varchar(63)
	,@effective_date datetime
AS
BEGIN
	SET NOCOUNT ON;
	
	SELECT 
		map_value
	from
		uvw_system_sales_model_map_universe WITH(NOLOCK)
	where
		system_id = @system_id
		and sales_model_id = @sales_model_id
		and map_set = @map_set
		AND
			(uvw_system_sales_model_map_universe.start_date<=@effective_date 
			AND 
				(uvw_system_sales_model_map_universe.end_date>=@effective_date 
				OR uvw_system_sales_model_map_universe.end_date IS NULL))
END

