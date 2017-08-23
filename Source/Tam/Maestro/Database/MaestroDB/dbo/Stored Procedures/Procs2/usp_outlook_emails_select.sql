CREATE PROCEDURE usp_outlook_emails_select
(
	@id Int
)
AS
SELECT
	*
FROM
	outlook_emails WITH(NOLOCK)
WHERE
	id = @id
