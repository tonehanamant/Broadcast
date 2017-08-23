-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetEmails_ForContact]
	@contact_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT 
		id,
		email_type_id,
		email,
		date_created,
		date_last_modified 
	FROM 
		emails (NOLOCK) 
	WHERE 
		id IN (
			SELECT email_id FROM contact_emails (NOLOCK) WHERE contact_id=@contact_id
		) 
	ORDER BY 
		date_last_modified
END
