
-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 3/26/2012
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].usp_PCS_GetGreatPlainsMappingsForCompanies
	@ids VARCHAR(MAX)
AS
BEGIN
	SELECT 
		gpm.*
	FROM 
		great_plains_mapping  gpm (NOLOCK)
	WHERE 
		gpm.advertiser_id IN (
			SELECT id FROM dbo.SplitIntegers(@ids)
		)
END