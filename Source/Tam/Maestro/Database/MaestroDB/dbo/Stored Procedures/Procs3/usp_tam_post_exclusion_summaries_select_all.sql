CREATE PROCEDURE usp_tam_post_exclusion_summaries_select_all
AS
SELECT
	*
FROM
	tam_post_exclusion_summaries WITH(NOLOCK)
