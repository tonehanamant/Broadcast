

-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetProposalsInRevisionChain]
	@proposal_id INT,
	@revised_proposal_id INT
AS
BEGIN
	SELECT 
		*
	FROM 
		proposals (NOLOCK)
	WHERE 
		id<>@proposal_id
		AND (
			id=@revised_proposal_id 
			OR original_proposal_id=@revised_proposal_id
		) 
END

