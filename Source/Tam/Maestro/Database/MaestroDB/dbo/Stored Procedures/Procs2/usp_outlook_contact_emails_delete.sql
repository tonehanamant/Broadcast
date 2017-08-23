CREATE PROCEDURE usp_outlook_contact_emails_delete
(
	@outlook_email_id		Int,
	@outlook_contact_id		Int)
AS
DELETE FROM
	outlook_contact_emails
WHERE
	outlook_email_id = @outlook_email_id
 AND
	outlook_contact_id = @outlook_contact_id
