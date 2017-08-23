CREATE PROCEDURE usp_contacts_select
(
	@id Int
)
AS
SELECT
	*
FROM
	contacts WITH(NOLOCK)
WHERE
	id = @id
