CREATE PROCEDURE usp_substitution_categories_select
(
	@id Int
)
AS
SELECT
	*
FROM
	substitution_categories WITH(NOLOCK)
WHERE
	id = @id
