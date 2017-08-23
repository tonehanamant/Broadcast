
-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].usp_PCS_GetAddressIdsForCompany
	@company_id INT
AS
BEGIN
	SELECT 
		address_id 
	FROM 
		company_addresses (NOLOCK)
	WHERE 
		company_id=@company_id
END