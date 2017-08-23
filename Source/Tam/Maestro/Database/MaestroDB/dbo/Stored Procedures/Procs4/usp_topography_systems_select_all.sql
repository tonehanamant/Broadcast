CREATE PROCEDURE usp_topography_systems_select_all
AS
SELECT
	*
FROM
	topography_systems WITH(NOLOCK)
