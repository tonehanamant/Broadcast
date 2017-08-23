
CREATE PROCEDURE [dbo].[usp_rating_source_rating_categories_select]
(
	@rating_source_id		TinyInt,
	@rating_category_id		Int
)
AS
SELECT
	*
FROM
	rating_source_rating_categories WITH(NOLOCK)
WHERE
	rating_source_id=@rating_source_id
	AND
	rating_category_id=@rating_category_id


