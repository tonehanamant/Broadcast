
-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].usp_PCS_GetAddressIdsForContact
	@contact_id INT
AS
BEGIN
	SELECT 
		address_id 
	FROM 
		contact_addresses (NOLOCK)
	WHERE 
		contact_id=@contact_id
END