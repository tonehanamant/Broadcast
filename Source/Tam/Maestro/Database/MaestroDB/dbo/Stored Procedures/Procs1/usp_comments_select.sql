
CREATE PROCEDURE [dbo].[usp_comments_select]
(
	@id Int
)
AS
SELECT
	*
FROM
	comments WITH(NOLOCK)
WHERE
	id = @id

