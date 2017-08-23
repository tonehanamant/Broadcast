CREATE PROCEDURE usp_reels_select_all
AS
SELECT
	*
FROM
	reels WITH(NOLOCK)
