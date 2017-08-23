CREATE PROCEDURE usp_email_types_select
(
	@id Int
)
AS
SELECT
	*
FROM
	email_types WITH(NOLOCK)
WHERE
	id = @id
