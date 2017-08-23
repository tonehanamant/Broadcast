CREATE PROCEDURE [dbo].[usp_TCS_GetTrafficDetailTopographies]
	@id INT
AS
BEGIN
	SELECT 
		tdt.*
	FROM 
		traffic_detail_topographies tdt (NOLOCK) 
		JOIN traffic_detail_weeks tdw (NOLOCK) ON tdw.id = tdt.traffic_detail_week_id
		JOIN traffic_details td (NOLOCK) ON td.id = tdw.traffic_detail_id
	WHERE
		td.traffic_id = @id
END
