
CREATE PROCEDURE [dbo].[usp_comment_types_select]
(
	@id Int
)
AS
SELECT
	*
FROM
	comment_types WITH(NOLOCK)
WHERE
	id = @id

