
CREATE PROCEDURE [dbo].[usp_rating_source_rating_categories_delete]
(
	@rating_source_id		TinyInt,
	@rating_category_id		Int)
AS
DELETE FROM
	rating_source_rating_categories
WHERE
	rating_source_id = @rating_source_id
 AND
	rating_category_id = @rating_category_id

