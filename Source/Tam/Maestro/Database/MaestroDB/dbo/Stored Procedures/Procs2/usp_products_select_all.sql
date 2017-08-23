CREATE PROCEDURE usp_products_select_all
AS
SELECT
	*
FROM
	products WITH(NOLOCK)
