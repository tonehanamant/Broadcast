-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetProposalDetailAudiences]
	@proposal_id INT
AS
BEGIN
	SET NOCOUNT ON;

    SELECT 
		pda.*,
		pd.universal_scaling_factor * pda.us_universe 'coverage_universe'
	FROM 
		proposal_detail_audiences pda	(NOLOCK)
		JOIN proposal_details pd		(NOLOCK) ON pd.id=pda.proposal_detail_id
	WHERE 
		pd.proposal_id=@proposal_id
END
