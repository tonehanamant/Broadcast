
-- =============================================
-- Author:		jcarsley
-- Create date: 10/26/2010
-- Description:	Selects the distinct name for an image map_set name for a logo in a Release report.
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectImageMapSetNames]
AS
BEGIN
	SET NOCOUNT ON;
    Select 
		distinct map_set
	from 
		system_image_sales_model_maps WITH(NOLOCK)
END

