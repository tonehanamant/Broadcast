CREATE PROCEDURE usp_default_company_post_report_options_select
(
	@company_id		Int,
	@post_buy_analysis_report_code		TinyInt
)
AS
SELECT
	*
FROM
	default_company_post_report_options WITH(NOLOCK)
WHERE
	company_id=@company_id
	AND
	post_buy_analysis_report_code=@post_buy_analysis_report_code

