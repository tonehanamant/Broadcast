
-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 3/26/2012
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].usp_PCS_GetGreatPlainsMappingsForProducts
	@ids VARCHAR(MAX)
AS
BEGIN
	SELECT 
		gpm.*
	FROM 
		great_plains_mapping  gpm (NOLOCK)
	WHERE 
		gpm.product_id IN (
			SELECT id FROM dbo.SplitIntegers(@ids)
		)
END