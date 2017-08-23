CREATE PROCEDURE [dbo].[usp_PTS_GetTopographyIDs]
	@tid as int
AS
BEGIN
	SELECT 
		DISTINCT tdt.topography_id 
	FROM
		traffic_detail_topographies tdt (NOLOCK)
		JOIN traffic_detail_weeks tdw (NOLOCK) ON tdw.id=tdt.traffic_detail_week_id
		JOIN traffic_details td (NOLOCK) ON td.id=tdw.traffic_detail_id
			AND td.id=@tid
	ORDER BY 
		tdt.topography_id;
END
