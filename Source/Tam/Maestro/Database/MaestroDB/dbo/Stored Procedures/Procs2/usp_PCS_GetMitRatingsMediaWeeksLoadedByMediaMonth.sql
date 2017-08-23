-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 12/23/2011
-- Description:	<Description,,>
-- =============================================
-- usp_PCS_GetMitRatingsMediaWeeksLoadedByMediaMonth 390,1
CREATE PROCEDURE [dbo].[usp_PCS_GetMitRatingsMediaWeeksLoadedByMediaMonth]
	@media_month_id INT,
	@rating_source_id TINYINT
AS
BEGIN
	SELECT COUNT(mw.id) FROM media_weeks mw (NOLOCK) WHERE mw.media_month_id=@media_month_id
	
	SELECT mm.* FROM media_months mm (NOLOCK) WHERE id=@media_month_id
		
	SELECT 
		* 
	FROM 
		dbo.GetFullMitRatingsWeeksByMediaMonth(@media_month_id,@rating_source_id) mw
	ORDER BY
		mw.start_date DESC
END
