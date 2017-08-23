CREATE PROCEDURE usp_release_product_descriptions_select_all
AS
SELECT
	*
FROM
	release_product_descriptions WITH(NOLOCK)
