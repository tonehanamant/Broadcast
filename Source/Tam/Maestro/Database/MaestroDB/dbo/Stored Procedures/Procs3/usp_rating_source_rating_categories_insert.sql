
CREATE PROCEDURE [dbo].[usp_rating_source_rating_categories_insert]
(
	@rating_source_id		TinyInt,
	@rating_category_id		Int
)
AS
INSERT INTO rating_source_rating_categories
(
	rating_source_id,
	rating_category_id
)
VALUES
(
	@rating_source_id,
	@rating_category_id
)


