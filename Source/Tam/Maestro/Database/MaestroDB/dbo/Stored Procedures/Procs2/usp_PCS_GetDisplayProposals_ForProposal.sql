-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 5/25/2010
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetDisplayProposals_ForProposal]
	@proposal_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT 
		dp.*
	FROM 
		uvw_display_proposals dp
	WHERE
		(dp.original_proposal_id=@proposal_id OR dp.id=@proposal_id)
		AND dp.proposal_status_id<>7
	ORDER BY 
		dp.id DESC
END
