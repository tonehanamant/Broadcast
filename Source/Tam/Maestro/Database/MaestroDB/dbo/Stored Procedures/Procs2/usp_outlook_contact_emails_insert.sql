CREATE PROCEDURE usp_outlook_contact_emails_insert
(
	@outlook_email_id		Int,
	@outlook_contact_id		Int
)
AS
INSERT INTO outlook_contact_emails
(
	outlook_email_id,
	outlook_contact_id
)
VALUES
(
	@outlook_email_id,
	@outlook_contact_id
)

