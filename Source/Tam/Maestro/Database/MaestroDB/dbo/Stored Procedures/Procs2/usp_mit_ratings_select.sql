CREATE PROCEDURE [dbo].[usp_mit_ratings_select]
(
	@id Int
)
AS
SELECT
	*
FROM
	dbo.mit_ratings WITH(NOLOCK)
WHERE
	id = @id
