
CREATE PROCEDURE [dbo].[usp_comment_types_select_all]
AS
SELECT
	*
FROM
	comment_types WITH(NOLOCK)

