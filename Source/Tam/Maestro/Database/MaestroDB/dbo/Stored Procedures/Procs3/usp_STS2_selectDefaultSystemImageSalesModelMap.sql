
-- =============================================
-- Author:		jcarsley
-- Create date: 11/17/2010
-- Description:	Gets the default image logo for a sales model, and map_set - by date
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectDefaultSystemImageSalesModelMap]
	 @sales_model_id int 
	,@map_set varchar(63)
	,@effective_date datetime
AS
BEGIN
	SET NOCOUNT ON;
	SELECT 
		id,
		system_id,
		image_id,
		sales_model_id,
		map_set,
		effective_date
	FROM
		system_image_sales_model_maps with(NOLOCK)
	where
		system_id is null
		and sales_model_id = @sales_model_id
		and map_set = @map_set
		and effective_date <= @effective_date
	order by
		effective_date desc
	
END

