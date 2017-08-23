-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 3/19/2014
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetRatingSourcesForPosts]
	@sales_model_id INT,
	@effective_date DATETIME -- optional
AS
BEGIN
	IF @effective_date IS NULL
		SET @effective_date = GETDATE();

	SELECT
		rs.*
	FROM
		dbo.rating_sources rs (NOLOCK)
		JOIN dbo.sales_model_rating_sources smrs (NOLOCK) ON rs.id=smrs.rating_source_id 
			AND smrs.sales_model_id=@sales_model_id
			AND smrs.use_for_posts=1
	WHERE
		rs.id IN (
			SELECT
				rating_source_id
			FROM
				rating_source_active_periods rsap (NOLOCK)
			WHERE
				rsap.start_date<=@effective_date AND (rsap.end_date>=@effective_date OR rsap.end_date IS NULL)
		)
END
