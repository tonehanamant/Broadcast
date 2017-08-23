CREATE PROCEDURE usp_topography_optimization_rules_select_all
AS
SELECT
	*
FROM
	topography_optimization_rules WITH(NOLOCK)
