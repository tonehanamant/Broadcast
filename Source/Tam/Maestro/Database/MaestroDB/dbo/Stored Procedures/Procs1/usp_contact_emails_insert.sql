CREATE PROCEDURE usp_contact_emails_insert
(
	@email_id		Int,
	@contact_id		Int
)
AS
INSERT INTO contact_emails
(
	email_id,
	contact_id
)
VALUES
(
	@email_id,
	@contact_id
)

