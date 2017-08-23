
-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].usp_PCS_GetProductIdsForCompany
	@company_id INT
AS
BEGIN
	SELECT 
		id 
	FROM 
		products (NOLOCK)
	WHERE 
		company_id=@company_id
END