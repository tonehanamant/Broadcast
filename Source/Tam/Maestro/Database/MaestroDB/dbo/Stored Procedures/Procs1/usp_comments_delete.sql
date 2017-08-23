
CREATE PROCEDURE [dbo].[usp_comments_delete]
(
	@id Int
)
AS
DELETE FROM comments WHERE id=@id

