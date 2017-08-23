

-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetChildProposalsForProposal]
	@proposal_id INT
AS
BEGIN
	SELECT 
		*
	FROM 
		proposals (NOLOCK)
	WHERE 
		original_proposal_id=@proposal_id
END

