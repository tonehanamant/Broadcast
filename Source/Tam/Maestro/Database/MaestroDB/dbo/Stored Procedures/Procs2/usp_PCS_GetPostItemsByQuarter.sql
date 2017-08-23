-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 9/10/2010
-- Description:	<Description,,>
-- =============================================
-- usp_PCS_GetPostItemsByQuarter 2011,1
CREATE PROCEDURE [dbo].[usp_PCS_GetPostItemsByQuarter]
	@year INT,
	@quarter INT
AS
BEGIN
	SELECT DISTINCT
		tp.id,
		tp.title
	FROM
		tam_post_proposals tpp (NOLOCK)
		JOIN tam_posts tp (NOLOCK) ON tp.id = tpp.tam_post_id
			AND tp.is_deleted=0
		JOIN proposals posting_plan (NOLOCK) ON posting_plan.id = tpp.posting_plan_proposal_id
		JOIN media_months mm (NOLOCK) ON mm.id = posting_plan.posting_media_month_id
	WHERE
		mm.year = @year
		AND CASE mm.month WHEN 1 THEN 1 WHEN 2 THEN 1 WHEN 3 THEN 1 WHEN 4 THEN 2 WHEN 5 THEN 2 WHEN 6 THEN 2 WHEN 7 THEN 3 WHEN 8 THEN 3 WHEN 9 THEN 3 WHEN 10 THEN 4 WHEN 11 THEN 4 WHEN 12 THEN 4 END = @quarter
	ORDER BY
		tp.title
END
