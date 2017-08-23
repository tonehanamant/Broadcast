-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_PCS_IsProposalInCampaign
	@proposal_id INT,
	@campaign_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT
		COUNT(*)
	FROM
		campaign_proposals (NOLOCK)
	WHERE
		campaign_id = @campaign_id
		AND proposal_id = @proposal_id
END
