-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 11/28/2012
-- Description:	Determines if a post is overnight or not. 
--				Any part of the post can be overnight for this to be true. 
--				If return value is > 1 it's overnight, false otherwise.
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_IsOverNightPost]
	@tam_post_id INT
AS
BEGIN
	DECLARE @num_overnight_plans INT
	SELECT 
		@num_overnight_plans = COUNT(*) 
	FROM 
		dbo.tam_post_proposals tpp (NOLOCK)
	WHERE 
		tpp.tam_post_id=@tam_post_id
		AND dbo.IsOvernightPlan(tpp.posting_plan_proposal_id)=1

	SELECT @num_overnight_plans
END
