CREATE PROCEDURE [dbo].[usp_system_statement_groups_select]
(
	@id Int
)
AS
SELECT
	*
FROM
	dbo.system_statement_groups WITH(NOLOCK)
WHERE
	id = @id
