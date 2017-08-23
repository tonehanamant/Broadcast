-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 8/26/2010
-- Description:	<Description,,>
-- =============================================
-- usp_PCS_GetDisplayPostProposalsByAdvertiser 39680
CREATE PROCEDURE [dbo].[usp_PCS_GetDisplayPostProposalsByAdvertiser]
	@advertiser_company_id INT
AS
BEGIN
	SELECT
		tp.*,
		tpp.*,
		tpp_ft.*,
		tpp_msa.*,
		dp.*
	FROM
		tam_post_proposals tpp (NOLOCK)
		JOIN tam_posts tp (NOLOCK) ON tp.id=tpp.tam_post_id
			AND tp.is_deleted=0
		JOIN uvw_display_proposals dp ON dp.id=tpp.posting_plan_proposal_id
		JOIN proposals p (NOLOCK) ON p.id=dp.id
		JOIN tam_post_proposals tpp_ft (NOLOCK) ON tpp_ft.tam_post_id=tpp.tam_post_id
			AND tpp_ft.posting_plan_proposal_id=tpp.posting_plan_proposal_id
			AND tpp_ft.post_source_code=1
		JOIN tam_post_proposals tpp_msa (NOLOCK) ON tpp_msa.tam_post_id=tpp.tam_post_id
			AND tpp_msa.posting_plan_proposal_id=tpp.posting_plan_proposal_id
			AND tpp_msa.post_source_code=2
	WHERE
		p.advertiser_company_id = @advertiser_company_id
		AND tpp.post_source_code = 0
END
