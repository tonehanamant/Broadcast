CREATE PROCEDURE usp_tam_post_report_options_delete
(
	@tam_post_id		Int,
	@post_buy_analysis_report_code		TinyInt)
AS
DELETE FROM
	tam_post_report_options
WHERE
	tam_post_id = @tam_post_id
 AND
	post_buy_analysis_report_code = @post_buy_analysis_report_code
