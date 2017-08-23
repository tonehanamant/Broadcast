CREATE PROCEDURE usp_contact_emails_select
(
	@email_id		Int,
	@contact_id		Int
)
AS
SELECT
	*
FROM
	contact_emails WITH(NOLOCK)
WHERE
	email_id=@email_id
	AND
	contact_id=@contact_id

