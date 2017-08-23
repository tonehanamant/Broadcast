-- =============================================
-- Author:		Stephen DeFusco
-- Create date: Unknown
-- Modified:	10/23/2013 - Stephen DeFusco, Added @rating_source_id parameter, completely changed.
--				Note: Special case for C3 ratings, we don't forecast it yet so this line was inserted until we do: CASE @rating_source_id WHEN 3 THEN 1 ELSE @rating_source_id END
-- =============================================
CREATE PROCEDURE [dbo].[usp_ARS_GetCurrentRatingsBaseMediaMonth]
	@rating_source_id TINYINT
AS
BEGIN
	SELECT TOP 1
		mm.*
	FROM
		dbo.forecasted_media_months fmm (NOLOCK)
		JOIN rating_source_rating_categories rsrc (NOLOCK) ON rsrc.rating_category_id=fmm.rating_category_id
			AND rsrc.rating_source_id=CASE @rating_source_id WHEN 3 THEN 1 ELSE @rating_source_id END
		JOIN media_months mm (NOLOCK) ON mm.id=fmm.base_media_month_id
	WHERE
		fmm.is_forecasted=1
	ORDER BY
		mm.start_date DESC
END
