CREATE PROCEDURE usp_system_statement_contact_infos_select
(
	@id Int
)
AS
SELECT
	*
FROM
	system_statement_contact_infos WITH(NOLOCK)
WHERE
	id = @id
