CREATE PROCEDURE usp_topography_dmas_select_all
AS
SELECT
	*
FROM
	topography_dmas WITH(NOLOCK)
