CREATE PROCEDURE usp_tam_post_analysis_reports_grp_trp_dmas_delete
(
	@tam_post_proposal_id		Int,
	@audience_id		Int,
	@dma_id		Int,
	@media_week_id		Int,
	@enabled		Bit)
AS
DELETE FROM
	tam_post_analysis_reports_grp_trp_dmas
WHERE
	tam_post_proposal_id = @tam_post_proposal_id
 AND
	audience_id = @audience_id
 AND
	dma_id = @dma_id
 AND
	media_week_id = @media_week_id
 AND
	enabled = @enabled
