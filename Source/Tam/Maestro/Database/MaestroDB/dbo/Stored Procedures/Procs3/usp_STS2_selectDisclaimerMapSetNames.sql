
-- =============================================
-- Author:		jcarsley
-- Create date: 10/26/2010
-- Description:	Selects the distinct name for a disclaimer line in a Release report.
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectDisclaimerMapSetNames]
AS
BEGIN
	SET NOCOUNT ON;

    Select 
		distinct map_set
	from 
		system_sales_model_maps WITH(NOLOCK)
	order by map_set
END

