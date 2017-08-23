CREATE PROCEDURE usp_email_messages_delete
(
	@id Int
)
AS
DELETE FROM email_messages WHERE id=@id
