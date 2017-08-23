CREATE PROCEDURE usp_email_messages_select
(
	@id Int
)
AS
SELECT
	*
FROM
	email_messages WITH(NOLOCK)
WHERE
	id = @id
