-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 2/21/2013
-- Description:	<Description,,>
-- =============================================
-- usp_PCS_GetDisplayPostsByMonth 373
CREATE PROCEDURE [dbo].[usp_PCS_GetDisplayPostsByMonth]
	@media_month_id INT
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
				AND mm.id=@media_month_id
		WHERE
			tpp.post_source_code = 0

	SELECT
		dp.*
	FROM
		uvw_display_posts dp
		JOIN #applicable_posts ap ON ap.tam_post_id = dp.id

	DROP TABLE #applicable_posts;
END
