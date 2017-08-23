CREATE PROCEDURE usp_zone_dmas_select_all
AS
SELECT
	*
FROM
	zone_dmas WITH(NOLOCK)
