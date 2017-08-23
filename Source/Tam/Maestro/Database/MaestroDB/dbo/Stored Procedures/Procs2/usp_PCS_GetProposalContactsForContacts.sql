
-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].usp_PCS_GetProposalContactsForContacts
	@ids VARCHAR(MAX)
AS
BEGIN
	SELECT 
		proposal_id,
		contact_id,
		date_created 
	FROM 
		proposal_contacts (NOLOCK)
	WHERE 
		contact_id IN (
			SELECT id FROM dbo.SplitIntegers(@ids)
		)
END
