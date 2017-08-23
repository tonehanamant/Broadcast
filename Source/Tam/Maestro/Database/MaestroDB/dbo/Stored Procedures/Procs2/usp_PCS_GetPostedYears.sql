-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 9/10/2010
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetPostedYears]
AS
BEGIN
	SELECT
		DISTINCT mm.year
	FROM
		proposals p (NOLOCK)
		JOIN media_months mm (NOLOCK) ON mm.id=p.posting_media_month_id
	ORDER BY
		mm.year DESC
END
