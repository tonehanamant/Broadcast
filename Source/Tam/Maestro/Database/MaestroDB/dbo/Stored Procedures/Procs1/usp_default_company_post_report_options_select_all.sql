CREATE PROCEDURE usp_default_company_post_report_options_select_all
AS
SELECT
	*
FROM
	default_company_post_report_options WITH(NOLOCK)
