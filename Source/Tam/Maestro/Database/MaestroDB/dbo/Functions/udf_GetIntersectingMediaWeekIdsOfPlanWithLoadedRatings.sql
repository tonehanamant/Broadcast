-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 5/13/2014
-- Description:	
-- =============================================
-- SELECT * FROM dbo.udf_GetIntersectingMediaWeekIdsOfPlanWithLoadedRatings(49596)
CREATE FUNCTION [dbo].[udf_GetIntersectingMediaWeekIdsOfPlanWithLoadedRatings]
(
	@proposal_id INT
)
RETURNS @return TABLE
(
	media_week_id INT
) 
AS
BEGIN
	DECLARE @rating_source_id TINYINT;
	DECLARE @media_month_id INT;
	SELECT 
		@media_month_id = p.posting_media_month_id,
		@rating_source_id = p.rating_source_id
	FROM 
		proposals p (NOLOCK) 
	WHERE 
		p.id=@proposal_id;
		
	INSERT INTO @return
		SELECT 
			mw.id 
		FROM 
			proposal_flights pf (NOLOCK) 
			JOIN media_weeks mw (NOLOCK) ON pf.start_date BETWEEN mw.start_date AND mw.end_date 
		WHERE 
			pf.proposal_id=@proposal_id
			AND pf.selected=1
		
		INTERSECT
		
		SELECT 
			mw.id 
		FROM 
			dbo.GetFullMitRatingsWeeksByMediaMonth(@media_month_id,@rating_source_id) mw;
			
	RETURN;
END
