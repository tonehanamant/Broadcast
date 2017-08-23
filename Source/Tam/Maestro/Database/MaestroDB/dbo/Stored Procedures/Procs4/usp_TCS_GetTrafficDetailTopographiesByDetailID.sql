CREATE PROCEDURE [dbo].[usp_TCS_GetTrafficDetailTopographiesByDetailID]
	@id INT
AS
BEGIN
	SELECT 
		tdt.*
	FROM 
		traffic_detail_topographies tdt (NOLOCK) 
		join traffic_detail_weeks tdw (NOLOCK) ON tdw.id = tdt.traffic_detail_week_id
	WHERE
		tdw.traffic_detail_id = @id
END
