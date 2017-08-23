
CREATE PROCEDURE [dbo].[usp_rating_source_rating_categories_select_all]
AS
SELECT
	*
FROM
	rating_source_rating_categories WITH(NOLOCK)

