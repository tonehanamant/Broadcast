CREATE PROCEDURE [dbo].[usp_TCS_GetTrafficDetailsByNetworkAndDaypart]
	@network_id INT,
	@traffic_id INT,
	@daypart_id INT
AS
BEGIN
	SELECT 
		td.*
	FROM 
		traffic_details td (NOLOCK) 
	WHERE 
		traffic_id = @traffic_id 
		and network_id = @network_id 
		and daypart_id = @daypart_id
END
