CREATE PROCEDURE usp_coverage_universes_select_all
AS
SELECT
	*
FROM
	coverage_universes WITH(NOLOCK)
