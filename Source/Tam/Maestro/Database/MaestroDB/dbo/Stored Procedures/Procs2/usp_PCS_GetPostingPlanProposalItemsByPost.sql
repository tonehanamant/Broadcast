-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 5/20/2010
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetPostingPlanProposalItemsByPost]
	@tam_post_id INT
AS
BEGIN
	SET NOCOUNT ON;

    SELECT 
		p.id,
		p.name 
	FROM 
		tam_post_proposals tpp (NOLOCK)
		JOIN proposals p (NOLOCK) ON p.id = tpp.posting_plan_proposal_id
	WHERE
		tpp.tam_post_id = @tam_post_id
		AND tpp.post_source_code = 0
	ORDER BY 
		id DESC
END
