
-- usp_PCS_GetDisplayAdvertiserPosts
CREATE PROCEDURE [dbo].[usp_PCS_GetDisplayAdvertiserPosts]
AS
BEGIN
	SELECT DISTINCT
		id,
		COUNT(*) 'total_posts'
	FROM
	(
		SELECT DISTINCT
			ordered_plan.advertiser_company_id as id,
			tpp.tam_post_id
		FROM
			tam_post_proposals tpp (NOLOCK)
			JOIN proposals posting_plan (NOLOCK) ON posting_plan.id = tpp.posting_plan_proposal_id
			JOIN proposals ordered_plan (NOLOCK) ON ordered_plan.id = posting_plan.original_proposal_id
	) tmp
	GROUP BY
		id
END
