
-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 7/25/2011
-- Description:	
-- =============================================
-- usp_PCS_IsPostAffiliatedWithAdvertiser 100001,40957
CREATE PROCEDURE [dbo].[usp_PCS_IsPostAffiliatedWithAdvertiser]
	@tam_post_id INT,
	@company_id INT
AS
BEGIN
	DECLARE @num_records INT
	
	SELECT 
		@num_records = COUNT(*)
	FROM
		tam_post_proposals tpp (NOLOCK)
		JOIN proposals posting_plan (NOLOCK) ON posting_plan.id = tpp.posting_plan_proposal_id
		JOIN proposals ordered_plan (NOLOCK) ON ordered_plan.id = posting_plan.original_proposal_id
	WHERE
		tpp.tam_post_id=@tam_post_id
		AND ordered_plan.advertiser_company_id=@company_id
		
	IF @num_records > 0
		SELECT CAST(1 AS BIT)
	ELSE
		SELECT CAST(0 AS BIT)
END
