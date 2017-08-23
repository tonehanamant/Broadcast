CREATE PROCEDURE [dbo].[usp_roles_select]
(
	@id Int
)
AS
SELECT
	*
FROM
	dbo.roles WITH(NOLOCK)
WHERE
	id = @id

