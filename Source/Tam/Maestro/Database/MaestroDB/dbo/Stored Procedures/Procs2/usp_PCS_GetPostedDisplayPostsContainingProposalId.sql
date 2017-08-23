-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 11/22/2010
-- Description:	<Description,,>
-- =============================================
-- usp_PCS_GetPostedDisplayPostsContainingProposalId
CREATE PROCEDURE [dbo].[usp_PCS_GetPostedDisplayPostsContainingProposalId]
	@proposal_id INT
AS
BEGIN
	SELECT
		dp.*
	FROM
		uvw_display_posts dp
	WHERE
		dp.id IN (
			SELECT DISTINCT
				tam_post_id
			FROM
				tam_post_proposals tpp (NOLOCK)
			WHERE
				tpp.post_completed IS NOT NULL
				AND tpp.posting_plan_proposal_id=@proposal_id
		)
END
