-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 5/20/2010
-- Description:	<Description,,>
-- =============================================
-- usp_PCS_GetDisplayPostsByQuarter 2011,4
CREATE PROCEDURE [dbo].[usp_PCS_GetDisplayPostsByQuarter]
	@year INT,
	@quarter INT
AS
BEGIN
	CREATE TABLE #applicable_posts (tam_post_id INT)
	INSERT INTO #applicable_posts
		SELECT DISTINCT
			tpp.tam_post_id
		FROM
			tam_post_proposals tpp (NOLOCK)
			JOIN proposals p (NOLOCK) ON p.id = tpp.posting_plan_proposal_id
			JOIN media_months mm (NOLOCK) ON mm.id=p.posting_media_month_id
		WHERE
			tpp.post_source_code = 0
			AND mm.year = @year
			AND CASE mm.month WHEN 1 THEN 1 WHEN 2 THEN 1 WHEN 3 THEN 1 WHEN 4 THEN 2 WHEN 5 THEN 2 WHEN 6 THEN 2 WHEN 7 THEN 3 WHEN 8 THEN 3 WHEN 9 THEN 3 WHEN 10 THEN 4 WHEN 11 THEN 4 WHEN 12 THEN 4 END = @quarter

	SELECT
		dp.*
	FROM
		uvw_display_posts dp
		JOIN #applicable_posts ap ON ap.tam_post_id = dp.id

	DROP TABLE #applicable_posts;
END
