-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 8/12/2013
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_TCS_GetTrafficDetailTopographiesByDetailIDAndFlight]
	@traffic_detail_id INT,
	@start_date DATETIME,
	@end_date DATETIME
AS
BEGIN
	SELECT 
		tdt.*
	FROM 
		traffic_detail_topographies tdt (NOLOCK) 
		JOIN traffic_detail_weeks tdw (NOLOCK) ON tdw.id = tdt.traffic_detail_week_id
	WHERE
		tdw.traffic_detail_id = @traffic_detail_id 
		AND (tdw.start_date <= @end_date AND tdw.end_date >= @start_date)
END
