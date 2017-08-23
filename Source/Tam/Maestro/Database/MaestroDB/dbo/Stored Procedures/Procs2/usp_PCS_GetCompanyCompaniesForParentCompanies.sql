
-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 10/7/2010
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetCompanyCompaniesForParentCompanies]
	@ids VARCHAR(MAX)
AS
BEGIN
	SELECT 
		cc.*
	FROM 
		company_companies cc (NOLOCK)
	WHERE 
		cc.parent_company_id IN (
			SELECT id FROM dbo.SplitIntegers(@ids)
		)
END