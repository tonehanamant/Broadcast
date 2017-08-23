CREATE PROCEDURE usp_default_company_post_report_options_insert
(
	@company_id		Int,
	@post_buy_analysis_report_code		TinyInt
)
AS
INSERT INTO default_company_post_report_options
(
	company_id,
	post_buy_analysis_report_code
)
VALUES
(
	@company_id,
	@post_buy_analysis_report_code
)

