
/***************************************************************************************************
** Date			Author			Description	
** ---------	----------		--------------------------------------------------------------------
** 10/12/2015	Abdul Sukkur	Traffic web - Get traffic network zone network map
*****************************************************************************************************/
CREATE FUNCTION [dbo].[udf_GetTrafficNetworkZoneNetworkMap]
(     
      @effective_date as datetime
)
RETURNS @traffic_zone_network_map TABLE
(
     traffic_network_id int,
     zone_network_id int
)
AS
BEGIN

	  insert into @traffic_zone_network_map
      (
		traffic_network_id,
		zone_network_id
      )
      (	SELECT network_id, CAST(map_value AS INT) 
		FROM dbo.udf_GetNetworkMapsAsOf(@effective_date) 
		WHERE map_set='PostReplace'
				UNION
		SELECT network_id, CAST(map_value AS INT) 
		FROM dbo.udf_GetNetworkMapsAsOf(@effective_date) 
		WHERE map_set='DaypartNetworks')
     RETURN;
END