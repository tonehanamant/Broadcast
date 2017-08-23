
-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetCompanyCompanyTypesForCompanies]
	@ids VARCHAR(MAX)
AS
BEGIN
	SELECT 
		cct.*
	FROM 
		company_company_types cct (NOLOCK)
	WHERE 
		cct.company_id IN (
			SELECT id FROM dbo.SplitIntegers(@ids)
		)
END