
CREATE PROCEDURE [dbo].[usp_comment_types_delete]
(
	@id Int
)
AS
DELETE FROM comment_types WHERE id=@id

