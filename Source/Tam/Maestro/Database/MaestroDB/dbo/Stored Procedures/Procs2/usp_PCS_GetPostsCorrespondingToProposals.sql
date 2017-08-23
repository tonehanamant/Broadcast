-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 7/21/2012
-- Description:	<Description,,>
-- =============================================
-- usp_PCS_GetPostsCorrespondingToProposals '32202'
CREATE PROCEDURE usp_PCS_GetPostsCorrespondingToProposals
	@proposal_ids VARCHAR(MAX)
AS
BEGIN
	SELECT DISTINCT
		tpp.tam_post_id,
		tpp.posting_plan_proposal_id
	FROM
		tam_post_proposals tpp (NOLOCK)
	WHERE
		tpp.posting_plan_proposal_id IN (
			SELECT id FROM dbo.SplitIntegers(@proposal_ids)
		)
END
