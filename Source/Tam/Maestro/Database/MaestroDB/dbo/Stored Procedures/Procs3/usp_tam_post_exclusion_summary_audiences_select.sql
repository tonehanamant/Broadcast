CREATE PROCEDURE usp_tam_post_exclusion_summary_audiences_select
(
	@tam_post_exclusion_summary_id		Int,
	@audience_id		Int
)
AS
SELECT
	*
FROM
	tam_post_exclusion_summary_audiences WITH(NOLOCK)
WHERE
	tam_post_exclusion_summary_id=@tam_post_exclusion_summary_id
	AND
	audience_id=@audience_id

