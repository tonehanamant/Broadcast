CREATE PROCEDURE usp_topography_system_histories_select_all
AS
SELECT
	*
FROM
	topography_system_histories WITH(NOLOCK)
