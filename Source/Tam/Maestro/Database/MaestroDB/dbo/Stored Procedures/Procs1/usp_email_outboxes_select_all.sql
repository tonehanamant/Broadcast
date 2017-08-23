CREATE PROCEDURE usp_email_outboxes_select_all
AS
SELECT
	*
FROM
	email_outboxes WITH(NOLOCK)
