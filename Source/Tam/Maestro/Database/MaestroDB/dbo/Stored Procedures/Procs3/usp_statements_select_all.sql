CREATE PROCEDURE usp_statements_select_all
AS
SELECT
	*
FROM
	statements WITH(NOLOCK)
