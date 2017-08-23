CREATE PROCEDURE usp_categories_select
(
	@id Int
)
AS
SELECT
	*
FROM
	categories WITH(NOLOCK)
WHERE
	id = @id
