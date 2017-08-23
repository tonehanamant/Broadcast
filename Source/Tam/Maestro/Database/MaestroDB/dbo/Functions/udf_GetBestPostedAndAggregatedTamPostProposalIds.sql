-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	Returns tam_post_proposals.id's based on the year/quarter/month paramters which:
--				- is MSA (post_source_code=2) and has been posted and aggregated OR is the TAM record (post_source_code=0)
--				The point is for a tam_post_id and posting_plan_proposal_id to return the BEST data (tam_post_proposals.id) available.
--				NOTE: This ONLY filters out all tam_posts marked "is_deleted". It does include "Spec" and "Official" posts as well as posts marked "Exclude from Year to Date".
-- =============================================
-- SELECT * FROM dbo.udf_GetBestPostedAndAggregatedTamPostProposalIds(2014,NULL,NULL)
CREATE FUNCTION [dbo].[udf_GetBestPostedAndAggregatedTamPostProposalIds]
(	
	@year INT,
	@quarter INT,	-- optional
	@month INT		-- optional
)
RETURNS TABLE 
AS
RETURN
(
	SELECT
		CASE WHEN tpp_m.post_completed IS NOT NULL AND tpp_m.aggregation_status_code=1 THEN tpp_m.id ELSE tpp_p.id END 'tam_post_proposal_id'
	FROM
		tam_post_proposals tpp_p		(NOLOCK)
		JOIN proposals p				(NOLOCK) ON p.id=tpp_p.posting_plan_proposal_id
		JOIN media_months mm			(NOLOCK) ON mm.id=p.posting_media_month_id
			AND @year = mm.[year]
			AND (@quarter IS NULL OR @quarter = CASE mm.[month] WHEN 1 THEN 1 WHEN 2 THEN 1 WHEN 3 THEN 1 WHEN 4 THEN 2 WHEN 5 THEN 2 WHEN 6 THEN 2 WHEN 7 THEN 3 WHEN 8 THEN 3 WHEN 9 THEN 3 WHEN 10 THEN 4 WHEN 11 THEN 4 WHEN 12 THEN 4 END)
			AND (@month IS NULL	  OR @month = mm.[month])
		JOIN tam_post_proposals tpp_m	(NOLOCK) ON tpp_m.tam_post_id=tpp_p.tam_post_id
			AND tpp_m.posting_plan_proposal_id=tpp_p.posting_plan_proposal_id
			AND tpp_m.post_source_code=2 --msa
		JOIN dbo.tam_posts tp			(NOLOCK) ON tp.id=tpp_p.tam_post_id
			AND tp.is_deleted=0			-- posts that haven't been market deleted
	WHERE
		tpp_p.post_source_code=0 --tam
)
