
-- =============================================
-- Author:		jcarsley
-- Create date: 10/27/2010
-- Description:	Gets the default disclaimer text for a sales model, and map_set
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectDefaultSystemSalesModelMap]
	 @sales_model_id int 
	,@map_set varchar(63)
	,@effective_date datetime
AS
BEGIN
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT 
		id,
		system_id,
		sales_model_id,
		map_set,
		map_value,
		effective_date
	FROM
		system_sales_model_maps with(NOLOCK)
	where
		system_id is null
		and sales_model_id = @sales_model_id
		and map_set = @map_set
		and effective_date <= @effective_date
	order by
		effective_date desc
	
END

