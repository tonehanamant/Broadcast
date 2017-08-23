CREATE PROCEDURE usp_email_outbox_details_select
(
	@id Int
)
AS
SELECT
	*
FROM
	email_outbox_details WITH(NOLOCK)
WHERE
	id = @id
