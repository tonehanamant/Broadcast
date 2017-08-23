-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 6/3/2015
-- Description:	Get's a list of all MSA posts, not marked "is_deleted", for the specified media month.
-- =============================================
-- usp_PCS_GetMsaTamPostProposalIdsByMediaMonth 401
CREATE PROCEDURE [dbo].[usp_PCS_GetMsaTamPostProposalIdsByMediaMonth]
	@media_month_id INT
AS
BEGIN
	SET NOCOUNT ON;
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

    SELECT
		tpp.id
	FROM
		tam_post_proposals tpp
		JOIN proposals p ON p.id=tpp.posting_plan_proposal_id
		JOIN tam_posts tp ON tp.id=tpp.tam_post_id
	WHERE
		p.posting_media_month_id=@media_month_id
		AND tp.is_msa=1
		AND tp.is_deleted=0
		AND tpp.post_source_code=2
	GROUP BY
		tpp.id
END