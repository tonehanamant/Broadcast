-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
-- usp_ACS_GetDisplayNetworkMaps 'Affidavits_%'
CREATE PROCEDURE [dbo].[usp_ACS_GetDisplayNetworkMaps]
	@map_set VARCHAR(15)
AS
BEGIN
	SELECT
		networks.code,
		network_maps.id,
		network_maps.network_id,
		network_maps.map_set,
		network_maps.map_value,
		network_maps.active,
		network_maps.flag,
		network_maps.effective_date,
		systems.id,
		systems.code
	FROM
		network_maps		(NOLOCK)
		JOIN networks		(NOLOCK) ON networks.id=network_maps.network_id
		LEFT JOIN systems	(NOLOCK) ON systems.id=CAST(REPLACE(network_maps.map_set, 'Affidavits_', '') AS INT)
	WHERE
		network_maps.map_set LIKE @map_set
	ORDER BY
		networks.code,
		network_maps.map_value,
		systems.code
END
