CREATE PROCEDURE [dbo].[usp_system_statement_groups_select_all]
AS
SELECT
	*
FROM
	dbo.system_statement_groups WITH(NOLOCK)
