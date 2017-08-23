CREATE PROCEDURE [dbo].[usp_msa_posts_select_all]
AS
SELECT
	*
FROM
	dbo.msa_posts WITH(NOLOCK)
