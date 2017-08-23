CREATE PROCEDURE [dbo].[usp_TCS_get_traffic_details_by_topography]
	@traffic_id INT,
	@topography_id INT
AS
BEGIN
	SELECT 
		tdt.*
	FROM 
		traffic_detail_topographies tdt (NOLOCK) 
		JOIN traffic_detail_weeks tdw (NOLOCK) ON tdw.id = tdt.traffic_detail_week_id
		JOIN traffic_details td (NOLOCK) ON td.id = tdw.traffic_detail_id
	WHERE 
		td.traffic_id = @traffic_id
		AND tdt.topography_id = @topography_id
END
