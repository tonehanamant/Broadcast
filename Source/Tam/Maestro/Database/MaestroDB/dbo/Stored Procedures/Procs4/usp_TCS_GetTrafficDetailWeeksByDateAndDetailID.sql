-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 
-- Last Mod:	8/12/2013, if dates were narrowed or widened within the week the original code would fail, modified line below
--		"and traffic_detail_weeks.start_date >= @startdate and traffic_detail_weeks.end_date <= @enddate" 
--		to this 
--		"AND (tdw.start_date <= @enddate AND tdw.end_date >= @startdate)"
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[usp_TCS_GetTrafficDetailWeeksByDateAndDetailID]
	@traffic_detail_id int,
	@startdate datetime,
	@enddate datetime
AS
BEGIN
	SELECT 
		tdw.*
	FROM 
		traffic_detail_weeks tdw (NOLOCK)
		JOIN traffic_details td (NOLOCK) ON td.id = tdw.traffic_detail_id
	WHERE 
		td.id = @traffic_detail_id
		AND (tdw.start_date <= @enddate AND tdw.end_date >= @startdate)
END