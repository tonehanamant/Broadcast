CREATE PROCEDURE usp_outlook_contact_emails_select_all
AS
SELECT
	*
FROM
	outlook_contact_emails WITH(NOLOCK)
