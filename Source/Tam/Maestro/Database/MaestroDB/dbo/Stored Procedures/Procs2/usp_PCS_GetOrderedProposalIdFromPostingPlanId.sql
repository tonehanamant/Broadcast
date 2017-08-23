-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
-- usp_PCS_GetOrderedProposalIdFromPostingPlanId 25902
CREATE PROCEDURE [dbo].[usp_PCS_GetOrderedProposalIdFromPostingPlanId]
	@proposal_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @id AS INT
    DECLARE @original_id AS INT
	
	SELECT 
		@original_id = op.original_proposal_id,
		@id = op.id
	FROM 
		proposals p (NOLOCK) 
		JOIN proposals op (NOLOCK) ON op.id=p.original_proposal_id
	WHERE 
		p.id=@proposal_id

	SELECT 
		id
	FROM 
		proposals (NOLOCK)
	WHERE 
		proposal_status_id=4
		AND (original_proposal_id=@id OR original_proposal_id=@original_id)
END
