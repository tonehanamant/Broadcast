CREATE PROCEDURE [dbo].[usp_mit_universes_select]
(
	@id Int
)
AS
SELECT
	*
FROM
	dbo.mit_universes WITH(NOLOCK)
WHERE
	id = @id
