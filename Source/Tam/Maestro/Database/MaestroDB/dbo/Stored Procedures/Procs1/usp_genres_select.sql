CREATE PROCEDURE usp_genres_select
(
	@id		Int
)
AS
BEGIN
SELECT
	*
FROM
	dbo.genres WITH(NOLOCK)
WHERE
	id=@id

END
