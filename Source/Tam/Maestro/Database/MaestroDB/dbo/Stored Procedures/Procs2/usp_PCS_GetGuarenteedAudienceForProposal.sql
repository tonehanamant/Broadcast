	
CREATE PROCEDURE [dbo].[usp_PCS_GetGuarenteedAudienceForProposal]
	@proposal_id INT
AS
BEGIN
    SELECT 
		pa.*
	FROM 
		proposals p (NOLOCK)
		JOIN proposal_audiences pa (NOLOCK) ON p.guarantee_type = pa.ordinal AND p.id = pa.proposal_id
	WHERE
		p.id = @proposal_id
END
