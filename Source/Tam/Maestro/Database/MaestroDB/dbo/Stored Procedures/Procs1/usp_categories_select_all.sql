CREATE PROCEDURE usp_categories_select_all
AS
SELECT
	*
FROM
	categories WITH(NOLOCK)
