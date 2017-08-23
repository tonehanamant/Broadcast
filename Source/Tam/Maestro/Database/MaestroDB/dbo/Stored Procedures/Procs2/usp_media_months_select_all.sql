CREATE PROCEDURE usp_media_months_select_all
AS
SELECT
	*
FROM
	media_months WITH(NOLOCK)
