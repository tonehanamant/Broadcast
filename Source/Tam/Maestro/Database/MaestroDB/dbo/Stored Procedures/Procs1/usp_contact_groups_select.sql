CREATE PROCEDURE usp_contact_groups_select
(
	@id Int
)
AS
SELECT
	*
FROM
	contact_groups WITH(NOLOCK)
WHERE
	id = @id
