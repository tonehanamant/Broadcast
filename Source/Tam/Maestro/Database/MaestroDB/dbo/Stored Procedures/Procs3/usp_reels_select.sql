CREATE PROCEDURE usp_reels_select
(
	@id Int
)
AS
SELECT
	*
FROM
	reels WITH(NOLOCK)
WHERE
	id = @id
