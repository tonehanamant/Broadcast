-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 5/20/2010
-- Description:	<Description,,>
-- =============================================
-- usp_PCS_GetPostItemsByAdvertiser
CREATE PROCEDURE [dbo].[usp_PCS_GetPostItemsByAdvertiser]
	@advertiser_company_id INT
AS
BEGIN
	SELECT DISTINCT
		tp.id,
		tp.title
	FROM
		tam_post_proposals tpp (NOLOCK)
		JOIN tam_posts tp (NOLOCK) ON tp.id = tpp.tam_post_id
			AND tp.is_deleted=0
		JOIN proposals posting_plan (NOLOCK) ON posting_plan.id = tpp.posting_plan_proposal_id
		JOIN proposals ordered_plan (NOLOCK) ON ordered_plan.id = posting_plan.original_proposal_id
	WHERE
		ordered_plan.advertiser_company_id = @advertiser_company_id
	ORDER BY
		tp.title
END
