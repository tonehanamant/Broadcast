-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 5/13/2014
-- Description:	Gets all tam_posts.id's that touch the year/quarter specified
-- =============================================
-- SELECT * FROM dbo.udf_GetTamPostsByFlight(2014,1)
CREATE FUNCTION [dbo].[udf_GetTamPostsByFlight]
(
	@year INT,
	@quarter INT
)
RETURNS @return TABLE
(
	tam_post_id INT
) 
AS
BEGIN
	INSERT INTO @return
		SELECT
			tpp.tam_post_id
		FROM
			dbo.tam_post_proposals tpp (NOLOCK)
			JOIN tam_posts tp (NOLOCK) ON tp.id=tpp.tam_post_id
			JOIN proposals p (NOLOCK) ON p.id=tpp.posting_plan_proposal_id
			JOIN media_months mm					(NOLOCK) ON mm.id=p.posting_media_month_id
				AND @year = mm.[year]
				AND (@quarter IS NULL OR @quarter = CASE mm.[month] WHEN 1 THEN 1 WHEN 2 THEN 1 WHEN 3 THEN 1 WHEN 4 THEN 2 WHEN 5 THEN 2 WHEN 6 THEN 2 WHEN 7 THEN 3 WHEN 8 THEN 3 WHEN 9 THEN 3 WHEN 10 THEN 4 WHEN 11 THEN 4 WHEN 12 THEN 4 END)
		WHERE
			tp.is_deleted=0			-- posts that haven't been market deleted
			AND tp.post_type_code=1 -- posts that have been marked "Official"
		GROUP BY
			tpp.tam_post_id;
		
	RETURN;
END
