CREATE PROCEDURE usp_topography_system_groups_select_all
AS
SELECT
	*
FROM
	topography_system_groups WITH(NOLOCK)
