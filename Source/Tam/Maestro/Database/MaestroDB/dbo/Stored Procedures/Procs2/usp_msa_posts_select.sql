CREATE PROCEDURE [dbo].[usp_msa_posts_select]
(
	@id Int
)
AS
SELECT
	*
FROM
	dbo.msa_posts WITH(NOLOCK)
WHERE
	id = @id
