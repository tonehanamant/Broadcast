CREATE PROCEDURE usp_tam_post_report_options_insert
(
	@tam_post_id		Int,
	@post_buy_analysis_report_code		TinyInt
)
AS
INSERT INTO tam_post_report_options
(
	tam_post_id,
	post_buy_analysis_report_code
)
VALUES
(
	@tam_post_id,
	@post_buy_analysis_report_code
)

