


CREATE PROCEDURE [dbo].[usp_BRS_GetNextVersionNumberByOrderID]
@orderID int

AS
BEGIN

	SET NOCOUNT ON;
	SELECT 
		(MAX(version_number) + 1)
	FROM
		cmw_traffic (nolock)
	WHERE
		id = @orderID or original_cmw_traffic_id = @orderID
END


