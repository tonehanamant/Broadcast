CREATE PROCEDURE usp_release_cpmlink_select_all
AS
SELECT
	*
FROM
	release_cpmlink WITH(NOLOCK)
