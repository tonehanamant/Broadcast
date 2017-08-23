CREATE PROCEDURE usp_rating_categories_select_all
AS
SELECT
	*
FROM
	rating_categories WITH(NOLOCK)
