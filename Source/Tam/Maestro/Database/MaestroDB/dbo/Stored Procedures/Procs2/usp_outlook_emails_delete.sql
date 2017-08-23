CREATE PROCEDURE usp_outlook_emails_delete
(
	@id Int
)
AS
DELETE FROM outlook_emails WHERE id=@id
