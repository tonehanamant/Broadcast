CREATE PROCEDURE usp_system_groups_select
(
	@id Int
)
AS
SELECT
	*
FROM
	system_groups WITH(NOLOCK)
WHERE
	id = @id
