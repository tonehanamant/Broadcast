-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 9/20/2013
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_FOG_GetMediaWeeksForMediaMonth]
	@media_month_id INT
AS
BEGIN
	SET NOCOUNT ON;

    SELECT 
		mw.* 
	FROM 
		media_weeks mw (NOLOCK) 
	WHERE 
		mw.media_month_id=@media_month_id
	ORDER BY
		mw.week_number
END
