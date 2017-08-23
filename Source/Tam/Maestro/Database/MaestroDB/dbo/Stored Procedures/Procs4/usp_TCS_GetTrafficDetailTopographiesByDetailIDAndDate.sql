CREATE Procedure [dbo].[usp_TCS_GetTrafficDetailTopographiesByDetailIDAndDate]
	@id INT,
	@start_date DATETIME
AS
BEGIN
	SELECT 
		tdt.*	
	FROM 
		traffic_detail_topographies tdt (NOLOCK) 
		JOIN traffic_detail_weeks tdw (NOLOCK) ON tdw.id = tdt.traffic_detail_week_id
	WHERE
		tdw.traffic_detail_id = @id 
		AND tdw.start_date = @start_date
END
