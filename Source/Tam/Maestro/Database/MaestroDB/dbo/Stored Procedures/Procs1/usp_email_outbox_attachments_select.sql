CREATE PROCEDURE usp_email_outbox_attachments_select
(
	@id Int
)
AS
SELECT
	*
FROM
	email_outbox_attachments WITH(NOLOCK)
WHERE
	id = @id
