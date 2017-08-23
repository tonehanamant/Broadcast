-- Author:       
-- Create date: 
-- Description: 
-- =============================================
CREATE PROCEDURE [dbo].[usp_ARS_GetDisplayDaypartsForNetworkForTraffic]
	@network_id INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		id,
		code,
		name,
		start_time,
		end_time,
		mon,
		tue,
		wed,
		thu,
		fri,
		sat,
		sun
	FROM
		vw_ccc_daypart
	WHERE
		id IN (
			SELECT daypart_id FROM network_traffic_dayparts (NOLOCK) WHERE nielsen_network_id IN (
				SELECT id FROM nielsen_networks (NOLOCK) WHERE nielsen_id IN (
					SELECT CAST(map_value AS INT) FROM network_maps (NOLOCK) WHERE map_set='Nielsen' AND network_id=@network_id
				)
			)
		)
END