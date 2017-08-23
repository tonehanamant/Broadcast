CREATE PROCEDURE usp_email_outbox_attachments_delete
(
	@id Int
)
AS
DELETE FROM email_outbox_attachments WHERE id=@id
