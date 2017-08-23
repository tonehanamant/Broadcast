

-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetContactEmailsForContacts]
	@ids VARCHAR(MAX)
AS
BEGIN
	SELECT 
		ce.*
	FROM 
		contact_emails ce (NOLOCK)
	WHERE 
		ce.contact_id IN (
			SELECT id FROM dbo.SplitIntegers(@ids)
		)
END
