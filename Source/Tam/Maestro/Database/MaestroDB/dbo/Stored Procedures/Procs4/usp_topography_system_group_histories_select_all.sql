CREATE PROCEDURE usp_topography_system_group_histories_select_all
AS
SELECT
	*
FROM
	topography_system_group_histories WITH(NOLOCK)
