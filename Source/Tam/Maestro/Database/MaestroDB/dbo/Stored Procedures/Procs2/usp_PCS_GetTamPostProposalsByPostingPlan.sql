-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 2/4/2011
-- Description:	
-- =============================================
CREATE PROCEDURE usp_PCS_GetTamPostProposalsByPostingPlan
	@posting_plan_proposal_id INT
AS
BEGIN
	SELECT
		tpp.*
	FROM
		tam_post_proposals tpp (NOLOCK)
	WHERE
		tpp.posting_plan_proposal_id=@posting_plan_proposal_id
END
