CREATE PROCEDURE [dbo].[usp_rating_category_groups_update]
(
	@id		TinyInt,
	@name		VarChar(63)
)
AS
UPDATE dbo.rating_category_groups SET
	name = @name
WHERE
	id = @id
