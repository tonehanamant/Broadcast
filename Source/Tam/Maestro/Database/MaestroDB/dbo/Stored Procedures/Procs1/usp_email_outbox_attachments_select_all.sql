CREATE PROCEDURE usp_email_outbox_attachments_select_all
AS
SELECT
	*
FROM
	email_outbox_attachments WITH(NOLOCK)
