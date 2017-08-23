

CREATE PROCEDURE [dbo].[usp_autoloaded_rating_categories_select_all]
AS
SELECT
	*
FROM
	dbo.autoloaded_rating_categories WITH(NOLOCK)

