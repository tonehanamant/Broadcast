-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetDisplayEmailsForContact]
	@contact_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT 
		email_types.name, 
		emails.email 
	FROM 
		emails (NOLOCK) 
		JOIN email_types (NOLOCK) ON email_types.id=emails.email_type_id 
	WHERE 
		emails.id IN (
			SELECT email_id FROM contact_emails (NOLOCK) WHERE contact_id=@contact_id
		)
END
