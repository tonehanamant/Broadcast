CREATE PROCEDURE usp_default_company_post_report_options_delete
(
	@company_id		Int,
	@post_buy_analysis_report_code		TinyInt)
AS
DELETE FROM
	default_company_post_report_options
WHERE
	company_id = @company_id
 AND
	post_buy_analysis_report_code = @post_buy_analysis_report_code
