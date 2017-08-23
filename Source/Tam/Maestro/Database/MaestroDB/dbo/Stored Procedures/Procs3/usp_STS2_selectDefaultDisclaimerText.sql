
-- =============================================
-- Author:		jcarsley
-- Create date: 10/27/2010
-- Description:	Gets the default disclaimer text for a sales model, and map_set
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectDefaultDisclaimerText]
	 @sales_model_id int 
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
		system_id is null
		and sales_model_id = @sales_model_id
		and map_set = @map_set
END

