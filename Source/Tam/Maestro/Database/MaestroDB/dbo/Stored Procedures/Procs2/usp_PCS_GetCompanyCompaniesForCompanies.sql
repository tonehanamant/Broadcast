
-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetCompanyCompaniesForCompanies]
	@ids VARCHAR(MAX)
AS
BEGIN
	SELECT 
		cc.*
	FROM 
		company_companies cc (NOLOCK)
	WHERE 
		cc.company_id IN (
			SELECT id FROM dbo.SplitIntegers(@ids)
		)
END