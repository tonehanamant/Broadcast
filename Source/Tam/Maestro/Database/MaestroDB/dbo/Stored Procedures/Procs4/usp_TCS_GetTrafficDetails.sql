CREATE PROCEDURE [dbo].[usp_TCS_GetTrafficDetails]
	@id INT
AS
BEGIN
	SELECT 
		td.*
	FROM
		traffic_details td (NOLOCK) 
	WHERE 
		td.traffic_id = @id
END
