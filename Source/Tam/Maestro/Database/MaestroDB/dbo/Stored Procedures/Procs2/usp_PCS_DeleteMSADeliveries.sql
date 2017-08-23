-- =============================================
-- Author:        Nicholas Kheynis
-- Create date: 8/5/2014
-- Description:   <Description,,>
-- =============================================
-- usp_PCS_DeleteMSADeliveries
CREATE PROCEDURE [dbo].[usp_PCS_DeleteMSADeliveries]
	  @tam_post_proposal_id INT
AS
BEGIN
	DECLARE @media_month_id INT

	SELECT 
		@media_month_id = p.posting_media_month_id 
	FROM 
		dbo.tam_post_proposals tpp
		JOIN dbo.proposals p(NOLOCK) ON p.id = tpp.posting_plan_proposal_id
	WHERE 
		tpp.id = @tam_post_proposal_id
		
	DELETE FROM
		dbo.msa_deliveries
	WHERE
		media_month_id = @media_month_id
		AND tam_post_proposal_id = @tam_post_proposal_id
END
