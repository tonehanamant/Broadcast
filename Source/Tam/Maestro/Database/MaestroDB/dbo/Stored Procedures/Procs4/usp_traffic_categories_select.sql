CREATE PROCEDURE usp_traffic_categories_select
(
	@id Int
)
AS
SELECT
	*
FROM
	traffic_categories WITH(NOLOCK)
WHERE
	id = @id
