CREATE PROCEDURE [dbo].[usp_REL_GetAvgTrafficDetailTopographiesByDetailID]
	@id int
AS
BEGIN
	SELECT
		tdt.traffic_detail_week_id,
		tdt.topography_id, 
		td.daypart_id,
		AVG(tdt.spots), 
		AVG(tdt.universe),
		AVG(tdt.rate),
		AVG(tdt.lookup_rate)
	FROM 
		traffic_detail_topographies tdt (NOLOCK) 
		join traffic_detail_weeks tdw (NOLOCK) ON tdw.id = tdt.traffic_detail_week_id
		join traffic_details td (NOLOCK) ON td.id = tdw.traffic_detail_id
	WHERE
		td.id = @id
		AND tdw.suspended = 0
	GROUP BY 
		tdt.topography_id, 
		td.daypart_id,
		tdt.traffic_detail_week_id
END
