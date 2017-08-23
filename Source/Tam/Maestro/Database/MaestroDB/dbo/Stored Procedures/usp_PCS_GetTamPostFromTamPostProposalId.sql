-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 3/23/2015
-- Description:	Get's the TamPost object based on it's child TamPostProposalId
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetTamPostFromTamPostProposalId]
	@tam_post_proposal_id INT
AS
BEGIN
	SET NOCOUNT ON;

    SELECT
		tp.*
	FROM
		tam_posts tp (NOLOCK)
	WHERE
		tp.id IN (
			SELECT tpp.tam_post_id FROM tam_post_proposals tpp (NOLOCK) WHERE tpp.id=@tam_post_proposal_id
		)
END