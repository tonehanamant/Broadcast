-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 10/25/2013
-- Description:	Checks to see if a base media month is available for a ratings source.
--				Note: Special case for C3 ratings, we don't forecast it yet so this line was inserted until we do: CASE @rating_source_id WHEN 3 THEN 1 ELSE @rating_source_id END
-- =============================================
CREATE PROCEDURE [dbo].[usp_ARS_IsBaseRatingsMediaMonthAvailable]
	@media_month_id INT,
	@rating_source_id TINYINT
AS
BEGIN
	SET NOCOUNT ON;

    SELECT
		mm.*
	FROM
		dbo.forecasted_media_months fmm (NOLOCK)
		JOIN rating_source_rating_categories rsrc (NOLOCK) ON rsrc.rating_category_id=fmm.rating_category_id
			AND rsrc.rating_source_id=CASE @rating_source_id WHEN 3 THEN 1 ELSE @rating_source_id END
		JOIN dbo.media_months mm (NOLOCK) ON mm.id=fmm.base_media_month_id
	WHERE
		fmm.base_media_month_id=@media_month_id
		AND fmm.is_forecasted=1
	ORDER BY
		mm.start_date DESC
END
