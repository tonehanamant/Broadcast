CREATE PROCEDURE usp_tam_post_exclusion_summary_audiences_delete
(
	@tam_post_exclusion_summary_id		Int,
	@audience_id		Int)
AS
DELETE FROM
	tam_post_exclusion_summary_audiences
WHERE
	tam_post_exclusion_summary_id = @tam_post_exclusion_summary_id
 AND
	audience_id = @audience_id
