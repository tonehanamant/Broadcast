CREATE PROCEDURE usp_emails_select
(
	@id Int
)
AS
SELECT
	*
FROM
	emails WITH(NOLOCK)
WHERE
	id = @id
