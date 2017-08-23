CREATE PROCEDURE [dbo].[usp_rating_category_groups_select]
(
	@id TinyInt
)
AS
SELECT
	*
FROM
	dbo.rating_category_groups WITH(NOLOCK)
WHERE
	id = @id
