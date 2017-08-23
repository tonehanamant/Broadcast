
-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetMaterialsForProducts]
	@ids VARCHAR(MAX)
AS
BEGIN
	SELECT 
		m.*
	FROM 
		materials m (NOLOCK)
	WHERE 
		m.product_id IN (
			SELECT id FROM dbo.SplitIntegers(@ids)
		)
END