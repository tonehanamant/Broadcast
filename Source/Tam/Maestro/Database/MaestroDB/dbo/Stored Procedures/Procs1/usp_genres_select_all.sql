CREATE PROCEDURE usp_genres_select_all
AS
BEGIN
SELECT
	*
FROM
	dbo.genres WITH(NOLOCK)
END
