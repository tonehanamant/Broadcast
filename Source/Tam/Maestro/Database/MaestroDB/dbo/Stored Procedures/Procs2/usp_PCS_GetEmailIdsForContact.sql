
-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].usp_PCS_GetEmailIdsForContact
	@contact_id INT
AS
BEGIN
	SELECT 
		email_id 
	FROM 
		contact_emails (NOLOCK)
	WHERE 
		contact_id=@contact_id
END