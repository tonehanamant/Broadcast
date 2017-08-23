-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 9/20/2013
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_FOG_GetTopographies]
AS
BEGIN
	SET NOCOUNT ON;

    SELECT 
		t.* 
	FROM 
		topographies t (NOLOCK) 
	WHERE 
		t.id IN (
			SELECT DISTINCT 
				si.topography_id 
			FROM 
				static_inventories si (NOLOCK)
		)
END
