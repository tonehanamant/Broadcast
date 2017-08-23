CREATE PROCEDURE [dbo].[usp_rating_category_groups_select_all]
AS
SELECT
	*
FROM
	dbo.rating_category_groups WITH(NOLOCK)
