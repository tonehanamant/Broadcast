
-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].usp_PCS_GetProposalItemsForCampaign
	@campaign_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT 
		id,
		name 
	FROM 
		proposals (NOLOCK)
	WHERE 
		id IN (
			SELECT proposal_id FROM campaign_proposals (NOLOCK) WHERE campaign_id=@campaign_id
		) 
	ORDER BY 
		id DESC
END

