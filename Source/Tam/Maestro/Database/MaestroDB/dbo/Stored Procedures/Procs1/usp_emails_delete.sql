CREATE PROCEDURE usp_emails_delete
(
	@id Int
)
AS
DELETE FROM emails WHERE id=@id
