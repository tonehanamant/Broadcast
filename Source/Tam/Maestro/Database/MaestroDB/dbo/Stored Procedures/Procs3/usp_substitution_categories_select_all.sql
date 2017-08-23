CREATE PROCEDURE usp_substitution_categories_select_all
AS
SELECT
	*
FROM
	substitution_categories WITH(NOLOCK)
