CREATE PROCEDURE [dbo].[usp_TCS_GetTrafficDetailsByNetworkID]
	@networkid INT,
	@trafficid INT
AS
BEGIN
	SELECT
		td.*
	FROM
		traffic_details td (NOLOCK) 
	WHERE
		td.network_id = @networkid 
		AND traffic_id = @trafficid
END
