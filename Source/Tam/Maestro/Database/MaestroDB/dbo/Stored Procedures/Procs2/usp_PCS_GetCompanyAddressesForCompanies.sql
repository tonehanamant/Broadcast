
-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetCompanyAddressesForCompanies]
	@ids VARCHAR(MAX)
AS
BEGIN
	SELECT 
		ca.*
	FROM 
		company_addresses ca (NOLOCK)
	WHERE 
		ca.company_id IN (
			SELECT id FROM dbo.SplitIntegers(@ids)
		)
END