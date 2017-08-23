
-- =============================================
-- Author:		jcarsley
-- Create date: 10/26/2010
-- Description:	Gets the disclaimer text for a sytem, sales model, and map_set
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectDisclaimerText]
	 @system_id int
	,@sales_model_id int 
	,@map_set varchar(63)
AS
BEGIN
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT 
		map_value
	from
		system_sales_model_maps WITH(NOLOCK)
	where
		system_id = @system_id
		and sales_model_id = @sales_model_id
		and map_set = @map_set
	order by
		effective_date desc
	
END

