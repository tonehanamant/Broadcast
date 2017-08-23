CREATE PROCEDURE usp_cmw_traffic_products_select
(
	@id Int
)
AS
SELECT
	*
FROM
	cmw_traffic_products WITH(NOLOCK)
WHERE
	id = @id
