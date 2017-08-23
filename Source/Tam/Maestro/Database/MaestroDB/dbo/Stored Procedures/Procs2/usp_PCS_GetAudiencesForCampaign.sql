-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 6/22/2011
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetAudiencesForCampaign]
	@campaign_id INT
AS
BEGIN
	SELECT DISTINCT
		pa.audience_id,
		a.name
	FROM
		campaign_proposals cp (NOLOCK)
		JOIN proposal_audiences pa (NOLOCK) ON pa.proposal_id=cp.proposal_id
		JOIN audiences a (NOLOCK) ON a.id=pa.audience_id
	WHERE
		cp.campaign_id=@campaign_id
	ORDER BY 
		a.name
END
