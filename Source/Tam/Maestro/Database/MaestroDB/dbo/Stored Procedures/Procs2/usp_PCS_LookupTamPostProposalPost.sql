-- =============================================
-- Author:      Nicholas Kheynis
-- Create date:	9/16/2014
-- Description: <Description,,>
-- =============================================
-- usp_PCS_LookupTamPostProposal 48224
CREATE PROCEDURE [dbo].[usp_PCS_LookupTamPostProposalPost]
      @tam_post_id INT,
      @posting_plan_proposal_id INT
AS
BEGIN	
	SELECT 
		tpp.*
	FROM 
		dbo.tam_post_proposals tpp
	WHERE
		tpp.tam_post_id = @tam_post_id and
		tpp.posting_plan_proposal_id = @posting_plan_proposal_id AND
		tpp.post_source_code = 0 
END
