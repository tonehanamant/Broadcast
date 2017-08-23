CREATE PROCEDURE usp_tam_post_exclusion_summary_audiences_select_all
AS
SELECT
	*
FROM
	tam_post_exclusion_summary_audiences WITH(NOLOCK)
