-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 9/12/2012
-- Description:	Gets all potential audiences for a particular post.
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetAudiencesForTamPost]
	@tam_post_id INT
AS
BEGIN
	SELECT DISTINCT
		pa.audience_id
	FROM
		tam_post_proposals tpp		(NOLOCK)
		JOIN proposals p			(NOLOCK) ON p.id=tpp.posting_plan_proposal_id
		JOIN proposal_audiences pa	(NOLOCK) ON pa.proposal_id=p.id
	WHERE
		tpp.tam_post_id=@tam_post_id
END
