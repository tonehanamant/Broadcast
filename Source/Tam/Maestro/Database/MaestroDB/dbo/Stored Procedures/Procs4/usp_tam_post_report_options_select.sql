CREATE PROCEDURE usp_tam_post_report_options_select
(
	@tam_post_id		Int,
	@post_buy_analysis_report_code		TinyInt
)
AS
SELECT
	*
FROM
	tam_post_report_options WITH(NOLOCK)
WHERE
	tam_post_id=@tam_post_id
	AND
	post_buy_analysis_report_code=@post_buy_analysis_report_code

