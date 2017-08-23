CREATE PROCEDURE [dbo].[usp_mit_ratings_select_all]
AS
SELECT
	*
FROM
	dbo.mit_ratings WITH(NOLOCK)
