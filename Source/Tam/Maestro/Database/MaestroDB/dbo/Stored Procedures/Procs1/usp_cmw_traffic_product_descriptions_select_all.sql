CREATE PROCEDURE [dbo].[usp_cmw_traffic_product_descriptions_select_all]
AS
SELECT
	*
FROM
	cmw_traffic_product_descriptions WITH(NOLOCK) ORDER BY product_description
