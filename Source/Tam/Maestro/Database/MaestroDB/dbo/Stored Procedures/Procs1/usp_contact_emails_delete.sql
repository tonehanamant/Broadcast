CREATE PROCEDURE usp_contact_emails_delete
(
	@email_id		Int,
	@contact_id		Int)
AS
DELETE FROM
	contact_emails
WHERE
	email_id = @email_id
 AND
	contact_id = @contact_id
