-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 1/29/2016
-- Description:	Returns a summary of affidavit posting state by media month and post source code (CAD PT, CAD FT, and MSA).
-- =============================================
-- Examples:
-- SELECT * FROM dbo.udf_GetAffidavitPostingSummary(405,0)
-- SELECT * FROM dbo.udf_GetAffidavitPostingSummary(405,1)
-- SELECT * FROM dbo.udf_GetAffidavitPostingSummary(405,2)
-- =============================================
CREATE FUNCTION [dbo].[udf_GetAffidavitPostingSummary]
(
	@media_month_id INT,
	@post_source_code TINYINT -- 0=CADENT Posts, 1=CADENT Fast Tracks, 2=MSA Posts
)
RETURNS @return TABLE 
(
	total_posts INT NOT NULL,
	num_posts_posted INT NOT NULL,
	num_posts_aggregated INT NOT NULL,
	perc_posts_posted FLOAT NOT NULL,
	perc_posts_aggregated FLOAT NOT NULL,
	perc_posts_posted_and_aggregated FLOAT NOT NULL
)
AS
BEGIN
	DECLARE @total_posts FLOAT = 0;
	DECLARE @total_posted FLOAT = 0;
	DECLARE @total_aggregated FLOAT = 0;
	DECLARE @total_posted_and_aggregated FLOAT = 0;

	SELECT
		@total_posts = COUNT(1),
		@total_posted = SUM(CASE WHEN tpp.post_completed IS NOT NULL THEN 1 ELSE 0 END),
		@total_aggregated = SUM(CASE WHEN tpp.aggregation_completed IS NOT NULL THEN 1 ELSE 0 END),
		@total_posted_and_aggregated = SUM(CASE WHEN tpp.post_completed IS NOT NULL AND tpp.aggregation_completed IS NOT NULL THEN 1 ELSE 0 END)
	FROM
		tam_post_proposals tpp (NOLOCK)
		JOIN tam_posts tp (NOLOCK) ON tp.id=tpp.tam_post_id
		JOIN proposals p (NOLOCK) ON p.id=tpp.posting_plan_proposal_id
	WHERE
		p.posting_media_month_id=@media_month_id
		AND tpp.post_source_code=@post_source_code
		AND tp.is_deleted=0
		AND tp.post_type_code=1
		AND 1=CASE @post_source_code WHEN 2 THEN tp.is_msa ELSE 1 END
	
	INSERT INTO @return
		SELECT
			@total_posts 'total_posts',
			@total_posted 'num_posts_posted',
			@total_aggregated 'num_posts_aggregated',
			(@total_posted / @total_posts) * 100.0 'perc_posts_posted',
			(@total_aggregated / @total_posts) * 100.0 'perc_posts_aggregated',
			(@total_posted_and_aggregated / @total_posts) * 100.0 'perc_posts_posted_and_aggregated'

	RETURN;
END