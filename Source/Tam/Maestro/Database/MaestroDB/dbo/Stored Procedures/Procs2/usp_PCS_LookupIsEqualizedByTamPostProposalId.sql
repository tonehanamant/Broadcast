-- =============================================
-- Author:        Nicholas Kheynis
-- Create date: 9/4/2014
-- Description:   <Description,,>
-- =============================================
-- usp_PCS_LookupIsEqualizedByTamPostProposalId 9
CREATE PROCEDURE [dbo].[usp_PCS_LookupIsEqualizedByTamPostProposalId]
	  @tam_post_proposal_id INT
AS
BEGIN
	SELECT 
		p.*
	FROM 
		dbo.proposals p
		JOIN dbo.tam_post_proposals tpp(NOLOCK) ON p.id = tpp.posting_plan_proposal_id
	WHERE
		tpp.id = @tam_post_proposal_id
END
