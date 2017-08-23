CREATE PROCEDURE usp_coverage_universe_details_select_all
AS
SELECT
	*
FROM
	coverage_universe_details WITH(NOLOCK)
