CREATE PROCEDURE usp_cmw_traffic_products_select_all
AS
SELECT
	*
FROM
	cmw_traffic_products WITH(NOLOCK)
