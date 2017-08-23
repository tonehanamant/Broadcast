CREATE PROCEDURE usp_tam_post_report_options_select_all
AS
SELECT
	*
FROM
	tam_post_report_options WITH(NOLOCK)
