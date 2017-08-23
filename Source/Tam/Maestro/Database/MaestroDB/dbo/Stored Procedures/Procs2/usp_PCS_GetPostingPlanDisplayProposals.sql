-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 1/10/2011
-- Description:	<Description,,>
-- =============================================
-- usp_PCS_GetPostingPlanDisplayProposals 2010,NULL,1
CREATE PROCEDURE [dbo].[usp_PCS_GetPostingPlanDisplayProposals]
	@year INT,
	@quarter INT,
	@month INT
AS
BEGIN
    SELECT 
		dp.*
	FROM 
		uvw_display_proposals dp
		JOIN proposals p (NOLOCK) ON p.id=dp.id
		JOIN media_months mm (NOLOCK) ON mm.id=p.posting_media_month_id
	WHERE
		(@year IS NULL OR @year=mm.[year])
		AND (@quarter IS NULL OR @quarter=CASE mm.[month] WHEN 1 THEN 1 WHEN 2 THEN 1 WHEN 3 THEN 1 WHEN 4 THEN 2 WHEN 5 THEN 2 WHEN 6 THEN 2 WHEN 7 THEN 3 WHEN 8 THEN 3 WHEN 9 THEN 3 WHEN 10 THEN 4 WHEN 11 THEN 4 WHEN 12 THEN 4 END)
		AND (@month IS NULL OR @month=mm.[month])
	ORDER BY 
		dp.id DESC
END
