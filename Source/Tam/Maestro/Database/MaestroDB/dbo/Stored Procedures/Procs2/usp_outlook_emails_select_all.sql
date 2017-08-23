CREATE PROCEDURE usp_outlook_emails_select_all
AS
SELECT
	*
FROM
	outlook_emails WITH(NOLOCK)
