CREATE PROCEDURE usp_products_select
(
	@id Int
)
AS
SELECT
	*
FROM
	products WITH(NOLOCK)
WHERE
	id = @id
