CREATE PROCEDURE [dbo].[usp_PCS_GetPostingPlanDisplayProposalsByAdvertiser]
	@company_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT 
		dp.*
	FROM 
		uvw_display_proposals dp
		JOIN proposals p (NOLOCK) ON p.id=dp.id
	WHERE
		p.advertiser_company_id = @company_id
		AND p.proposal_status_id = 7
	ORDER BY 
		dp.id DESC
END
