CREATE PROCEDURE usp_topographies_select_all
AS
SELECT
	*
FROM
	topographies WITH(NOLOCK)
