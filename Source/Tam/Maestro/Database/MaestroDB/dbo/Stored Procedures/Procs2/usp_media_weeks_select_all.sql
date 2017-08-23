CREATE PROCEDURE usp_media_weeks_select_all
AS
SELECT
	*
FROM
	media_weeks WITH(NOLOCK)
