CREATE PROCEDURE usp_traffic_categories_select_all
AS
SELECT
	*
FROM
	traffic_categories WITH(NOLOCK)
