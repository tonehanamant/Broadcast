CREATE PROCEDURE usp_email_outbox_details_delete
(
	@id Int
)
AS
DELETE FROM email_outbox_details WHERE id=@id
