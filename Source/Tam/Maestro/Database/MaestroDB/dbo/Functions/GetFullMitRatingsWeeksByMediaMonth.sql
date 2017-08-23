-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 9/10/2012
-- Description:	
-- =============================================
CREATE FUNCTION [dbo].[GetFullMitRatingsWeeksByMediaMonth]
(	
	@media_month_id INT,
	@rating_source_id TINYINT
)
RETURNS @return TABLE
(
	id INT, 
	media_month_id INT, 
	week_number INT, 
	start_date DATETIME, 
	end_date DATETIME
)
AS
BEGIN
	INSERT INTO @return
		SELECT
			mw.*
		FROM
		(
			SELECT DISTINCT
				mw.id 'media_week_id',
				mr.rating_date
			FROM 
				mit_ratings mr (NOLOCK)
				JOIN rating_source_rating_categories rsrc (NOLOCK) ON rsrc.rating_category_id=mr.rating_category_id
					AND rsrc.rating_source_id=@rating_source_id
				JOIN media_weeks mw (NOLOCK) ON mr.rating_date BETWEEN mw.start_date AND mw.end_date
			WHERE
				mr.media_month_id=@media_month_id
		) tmp
		JOIN media_weeks mw (NOLOCK) ON mw.id=tmp.media_week_id
		GROUP BY
			id, media_month_id, week_number, start_date, end_date
		HAVING
			COUNT(mw.id) > 1
	RETURN;
END
