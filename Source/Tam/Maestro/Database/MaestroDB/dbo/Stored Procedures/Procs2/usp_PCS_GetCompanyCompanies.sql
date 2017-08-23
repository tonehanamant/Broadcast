
-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetCompanyCompanies]
	@company_id INT
AS
BEGIN
	SELECT 
		cc.*
	FROM 
		company_companies cc (NOLOCK)
	WHERE 
		cc.company_id=@company_id
END

