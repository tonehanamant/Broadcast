CREATE PROCEDURE usp_tam_post_analysis_reports_dmas_select_all
AS
SELECT
	*
FROM
	tam_post_analysis_reports_dmas WITH(NOLOCK)
