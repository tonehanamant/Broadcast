CREATE PROCEDURE usp_email_messages_select_all
AS
SELECT
	*
FROM
	email_messages WITH(NOLOCK)
