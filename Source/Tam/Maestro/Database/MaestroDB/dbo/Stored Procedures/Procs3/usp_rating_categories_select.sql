CREATE PROCEDURE usp_rating_categories_select
(
	@id		Int
)
AS
SELECT
	*
FROM
	rating_categories WITH(NOLOCK)
WHERE
	id=@id

