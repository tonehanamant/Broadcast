CREATE PROCEDURE usp_tam_post_reports_select_all
AS
SELECT
	*
FROM
	tam_post_reports WITH(NOLOCK)
