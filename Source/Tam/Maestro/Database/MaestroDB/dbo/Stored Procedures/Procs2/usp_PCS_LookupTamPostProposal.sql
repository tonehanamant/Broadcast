-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 7/28/2010
-- Description:	Retrieves a TamPostProposal record based on the tam_post_id and posting_plan_proposal_id.
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_LookupTamPostProposal]
	@tam_post_id INT,
	@posting_plan_proposal_id INT
AS
BEGIN
    SELECT
		tpp.*
	FROM
		tam_post_proposals tpp (NOLOCK)
	WHERE
		tpp.tam_post_id = @tam_post_id
		AND tpp.posting_plan_proposal_id = @posting_plan_proposal_id
END
