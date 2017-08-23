CREATE PROCEDURE usp_contact_emails_select_all
AS
SELECT
	*
FROM
	contact_emails WITH(NOLOCK)
