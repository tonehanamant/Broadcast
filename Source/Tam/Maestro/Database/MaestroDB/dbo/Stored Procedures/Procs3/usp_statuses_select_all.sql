CREATE PROCEDURE usp_statuses_select_all
AS
SELECT
	*
FROM
	statuses WITH(NOLOCK)
