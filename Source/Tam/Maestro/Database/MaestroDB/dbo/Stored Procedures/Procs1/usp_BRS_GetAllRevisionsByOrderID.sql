
CREATE PROCEDURE [dbo].[usp_BRS_GetAllRevisionsByOrderID]
@cmwTrafficID int

AS
BEGIN

	SET NOCOUNT ON;

	declare @originalCMWTrafficID int
	set @originalCMWTrafficID = (select original_cmw_traffic_id from cmw_traffic where id = @cmwTrafficID)

	SELECT 
		cmw_traffic.id,
		CASE 
			WHEN 
				original_cmw_traffic_id IS NULL 
			THEN 
				CAST(cmw_traffic.id AS varchar) 
			ELSE 
				CAST(original_cmw_traffic_id AS varchar) + '-R' + CAST(cmw_traffic.version_number AS varchar) 
			END AS display_id
	FROM
		cmw_traffic (nolock)
	WHERE
		(((@originalCMWTrafficID is null) 
	AND cmw_traffic.original_cmw_traffic_id = @cmwTrafficID)
	OR
		(cmw_traffic.id <> @cmwTrafficID
	AND
		(cmw_traffic.id = @originalCMWTrafficID 
	OR
		cmw_traffic.original_cmw_traffic_id = @originalCMWTrafficID)))

END