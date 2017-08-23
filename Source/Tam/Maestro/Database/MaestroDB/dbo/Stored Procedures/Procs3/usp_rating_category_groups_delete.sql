CREATE PROCEDURE [dbo].[usp_rating_category_groups_delete]
(
	@id TinyInt
)
AS
DELETE FROM dbo.rating_category_groups WHERE id=@id
