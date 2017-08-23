-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 8/25/2014
-- Description: Get's the proposal linkage for a given linkage type and ordered proposal.
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetProposalLinkagesByPrimaryProposal]
	@primary_proposal_id INT,
	@proposal_linkage_type TINYINT
AS
BEGIN
	SELECT
		pl.*
	FROM
		dbo.proposal_linkages pl (NOLOCK)
	WHERE
		pl.proposal_linkage_type=@proposal_linkage_type
		AND pl.primary_proposal_id=@primary_proposal_id;
END
