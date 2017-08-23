-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 7/5/2011
-- Description: 
-- =============================================
CREATE PROCEDURE usp_PCS_GetContactGroupContactsForContacts
	@ids VARCHAR(MAX)
AS
BEGIN
	SELECT
		cgc.*
	FROM
		contact_group_contacts cgc (NOLOCK)
	WHERE
		cgc.contact_id IN (
			SELECT id FROM dbo.SplitIntegers(@ids)
		)
END
