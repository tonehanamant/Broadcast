CREATE PROCEDURE usp_email_outboxes_select
(
	@id Int
)
AS
SELECT
	*
FROM
	email_outboxes WITH(NOLOCK)
WHERE
	id = @id
