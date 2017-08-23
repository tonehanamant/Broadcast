CREATE Procedure [dbo].[usp_TCS_GetTrafficDetailTopographiesForNetworkAndTopo]
	@network_id INT, 
	@topography_id INT,
	@traffic_id INT
AS
BEGIN
	SELECT 
		tdt.*
	FROM 
		traffic_detail_topographies tdt (NOLOCK) 
		JOIN traffic_detail_weeks tdw (NOLOCK) on tdw.id = tdt.traffic_detail_week_id
		JOIN traffic_details td (NOLOCK) on td.id = tdw.traffic_detail_id
	WHERE 
		td.traffic_id = @traffic_id 
		AND tdt.topography_id = @topography_id 
		AND td.network_id = @network_id
END
