CREATE PROCEDURE usp_topography_businesses_select_all
AS
SELECT
	*
FROM
	topography_businesses WITH(NOLOCK)
