CREATE PROCEDURE usp_releases_select_all
AS
SELECT
	*
FROM
	releases WITH(NOLOCK)
