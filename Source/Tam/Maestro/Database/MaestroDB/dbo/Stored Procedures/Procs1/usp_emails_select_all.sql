CREATE PROCEDURE usp_emails_select_all
AS
SELECT
	*
FROM
	emails WITH(NOLOCK)
