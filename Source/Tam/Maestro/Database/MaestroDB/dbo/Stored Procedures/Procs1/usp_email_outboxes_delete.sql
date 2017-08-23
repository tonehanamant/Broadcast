CREATE PROCEDURE usp_email_outboxes_delete
(
	@id Int
)
AS
DELETE FROM email_outboxes WHERE id=@id
