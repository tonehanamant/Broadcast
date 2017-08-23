
-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetAddressesForContacts]
	@ids VARCHAR(MAX)
AS
BEGIN
	SELECT
		a.*
	FROM 
		addresses a (NOLOCK)
	WHERE 
		a.id IN (
			SELECT address_id FROM contact_addresses (NOLOCK) WHERE contact_id IN (
				SELECT id FROM dbo.SplitIntegers(@ids)
			)
		)
END

