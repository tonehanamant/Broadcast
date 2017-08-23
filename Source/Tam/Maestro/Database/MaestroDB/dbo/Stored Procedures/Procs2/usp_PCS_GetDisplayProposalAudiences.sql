-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 12/7/2012
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_PCS_GetDisplayProposalAudiences
	@proposal_id INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		pa.ordinal,
		pa.universe,
		a.*
	FROM 
		dbo.proposal_audiences pa (NOLOCK)
		JOIN dbo.audiences a (NOLOCK) ON a.id=pa.audience_id
	WHERE
		pa.proposal_id=@proposal_id
END
