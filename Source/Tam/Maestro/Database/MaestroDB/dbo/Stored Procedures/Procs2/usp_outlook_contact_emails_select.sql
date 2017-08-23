CREATE PROCEDURE usp_outlook_contact_emails_select
(
	@outlook_email_id		Int,
	@outlook_contact_id		Int
)
AS
SELECT
	*
FROM
	outlook_contact_emails WITH(NOLOCK)
WHERE
	outlook_email_id=@outlook_email_id
	AND
	outlook_contact_id=@outlook_contact_id

