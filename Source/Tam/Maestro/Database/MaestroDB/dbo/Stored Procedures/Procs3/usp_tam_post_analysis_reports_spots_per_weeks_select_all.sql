CREATE PROCEDURE usp_tam_post_analysis_reports_spots_per_weeks_select_all
AS
SELECT
	*
FROM
	tam_post_analysis_reports_spots_per_weeks WITH(NOLOCK)
