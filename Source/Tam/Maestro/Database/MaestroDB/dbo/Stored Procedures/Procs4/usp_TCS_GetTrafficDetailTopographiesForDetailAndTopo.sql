CREATE PROCEDURE [dbo].[usp_TCS_GetTrafficDetailTopographiesForDetailAndTopo]
	@traffic_detail_id INT, 
	@topography_id INT
AS
BEGIN
	SELECT 
		tdt.*
	FROM 
		traffic_detail_topographies tdt (NOLOCK)
		JOIN traffic_detail_weeks tdw (NOLOCK) ON tdw.id = tdt.traffic_detail_week_id
	WHERE 
		tdw.traffic_detail_id = @traffic_detail_id 
		AND tdt.topography_id = @topography_id
END
