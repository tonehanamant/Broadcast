CREATE PROCEDURE usp_tam_post_analysis_reports_dmas_delete
(
	@tam_post_proposal_id		Int,
	@audience_id		Int,
	@enabled		Bit)
AS
DELETE FROM
	tam_post_analysis_reports_dmas
WHERE
	tam_post_proposal_id = @tam_post_proposal_id
 AND
	audience_id = @audience_id
 AND
	enabled = @enabled
