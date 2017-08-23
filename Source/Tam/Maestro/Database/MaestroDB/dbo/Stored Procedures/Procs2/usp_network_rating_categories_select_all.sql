CREATE PROCEDURE usp_network_rating_categories_select_all
AS
SELECT
	*
FROM
	network_rating_categories WITH(NOLOCK)
