
CREATE PROCEDURE [dbo].[usp_comments_select_all]
AS
SELECT
	*
FROM
	comments WITH(NOLOCK)

