CREATE PROCEDURE usp_dmas_select_all
AS
SELECT
	*
FROM
	dmas WITH(NOLOCK)
