
CREATE PROCEDURE [dbo].[usp_comment_types_update]
(
	@id		Int,
	@name		VarChar(50)
)
AS
UPDATE comment_types SET
	name = @name
WHERE
	id = @id


