-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_CanDeleteProposal]
	@proposal_id INT
AS
BEGIN
	SET NOCOUNT ON;

    DECLARE @return AS INT

	SET @return = 0
	
	SET @return = @return + (
		SELECT COUNT(1) FROM campaign_proposals (NOLOCK) WHERE proposal_id=@proposal_id
	)
	SET @return = @return + (
		SELECT COUNT(1) FROM proposal_marry_candidates (NOLOCK) WHERE proposal_id=@proposal_id
	)
	SET @return = @return + (
		SELECT COUNT(1) FROM proposal_media_month_marriage_mappings (NOLOCK) WHERE proposal_id=@proposal_id
	)
	SET @return = @return + (
		SELECT COUNT(1) FROM proposal_proposals (NOLOCK) WHERE parent_proposal_id=@proposal_id OR child_proposal_id=@proposal_id
	)
	SET @return = @return + (
		SELECT COUNT(1) FROM traffic_proposals tp (NOLOCK) WHERE tp.proposal_id=@proposal_id
	)
	SET @return = @return + (
		SELECT COUNT(1) FROM static_orders (NOLOCK) WHERE proposal_id=@proposal_id
	)
	SET @return = @return + (
		SELECT COUNT(1) FROM release_cpmlink (NOLOCK) WHERE proposal_id=@proposal_id
	)
	SET @return = @return + (
		SELECT COUNT(1) FROM tam_post_proposals (NOLOCK) WHERE posting_plan_proposal_id=@proposal_id
	)
	SET @return = @return + (
		SELECT COUNT(1) FROM proposals (NOLOCK) WHERE id=@proposal_id AND proposal_status_id IN (3,4,5)
	)
	SET @return = @return + (
		SELECT COUNT(1) FROM proposals (NOLOCK) WHERE original_proposal_id=@proposal_id
	)
	SET @return = @return + (
		SELECT COUNT(1) FROM proposal_linkages (NOLOCK) WHERE primary_proposal_id=@proposal_id
	)
		
	SELECT 
		CASE WHEN @return > 0 THEN CAST(0 AS BIT) ELSE CAST(1 AS BIT) END 'can_delete'
END
