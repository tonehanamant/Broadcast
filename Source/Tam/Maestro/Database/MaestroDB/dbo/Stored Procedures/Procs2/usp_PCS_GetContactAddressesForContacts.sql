-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 9/20/2010
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetContactAddressesForContacts]
	@ids VARCHAR(MAX)
AS
BEGIN
	SELECT
		ca.*
	FROM
		contact_addresses ca (NOLOCK)
	WHERE
		ca.contact_id IN (
			SELECT id FROM dbo.SplitIntegers(@ids)
		)
END
