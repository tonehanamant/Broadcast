CREATE PROCEDURE usp_cmw_traffic_product_descriptions_select
(
	@id Int
)
AS
SELECT
	*
FROM
	cmw_traffic_product_descriptions WITH(NOLOCK)
WHERE
	id = @id
