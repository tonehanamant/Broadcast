-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 12/20/2012
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_IsMarriedProposal]
	@proposal_id INT
AS
BEGIN
	SET NOCOUNT ON;

    SELECT
		COUNT(1)
	FROM
		dbo.proposal_proposals pp (NOLOCK)
	WHERE
		pp.parent_proposal_id=@proposal_id
END
