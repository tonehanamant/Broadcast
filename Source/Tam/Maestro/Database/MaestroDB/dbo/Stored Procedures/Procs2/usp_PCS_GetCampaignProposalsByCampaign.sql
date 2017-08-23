-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
-- usp_PCS_GetCampaignProposalsByCampaign 22
CREATE PROCEDURE [dbo].[usp_PCS_GetCampaignProposalsByCampaign]
	@campaign_id int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT
		cp.*
	FROM
		campaign_proposals cp (NOLOCK) 
		JOIN proposals p (NOLOCK) ON p.id=cp.proposal_id
	WHERE
		cp.campaign_id=@campaign_id
	ORDER BY
		p.start_date
END
