CREATE PROCEDURE usp_release_product_descriptions_select
(
	@id Int
)
AS
SELECT
	*
FROM
	release_product_descriptions WITH(NOLOCK)
WHERE
	id = @id
