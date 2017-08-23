CREATE PROCEDURE usp_network_rating_categories_select
(
	@id Int
)
AS
SELECT
	*
FROM
	network_rating_categories WITH(NOLOCK)
WHERE
	id = @id
