-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 5/25/2010
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetDisplayProposals_ForCampaign]
	@campaign_id INT
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
		dp.id IN (
			SELECT proposal_id FROM campaign_proposals (NOLOCK) WHERE campaign_id=@campaign_id
		)
	ORDER BY 
		dp.id DESC
END
