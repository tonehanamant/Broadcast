CREATE PROCEDURE [dbo].[usp_roles_select_all]
AS
SELECT
	*
FROM
	dbo.roles WITH(NOLOCK)
