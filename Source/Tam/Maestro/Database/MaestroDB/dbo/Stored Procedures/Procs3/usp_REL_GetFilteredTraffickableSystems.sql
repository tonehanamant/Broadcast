
/***************************************************************************************************
** Date         Author          Description   
** ---------    ----------		-----------------------------------------------------
** XXXX			XXX
** 03/13/2017	Abdul Sukkur	Added new columns to systems 
*****************************************************************************************************/
CREATE PROCEDURE [dbo].[usp_REL_GetFilteredTraffickableSystems]
(
	@traffic_ids varchar(MAX),
	@topography_id int,
	@effective_date datetime
)

AS

declare @query as varchar(max);

CREATE TABLE #Network_Temp (traffic_id int, network_id int);
set @query = 'INSERT INTO #Network_Temp (traffic_id, network_id) SELECT DISTINCT traffic_id, network_id from 
traffic_details WITH (NOLOCK) WHERE traffic_details.traffic_id in (' + @traffic_ids + ')';
print @query;
exec (@query);

set @query = '
SELECT 
	DISTINCT 
	systems.id, 
	systems.code,
	systems.name, 
	systems.location, 
	systems.spot_yield_weight, 
	systems.traffic_order_format, 
	systems.flag, 
	systems.active, 
	systems.effective_date,
	count(distinct #Network_Temp.traffic_id)
FROM 
	dbo.udf_GetTrafficZoneInformationByTopographyAsOf(' + CAST(@topography_id as varchar(5)) + ', ''' + convert(varchar, @effective_date, 101) + ''', 1) traffic_systems
join
	systems WITH (NOLOCK) on systems.id = traffic_systems.system_id
join
	#Network_Temp WITH (NOLOCK) on #Network_Temp.network_id = traffic_systems.traffic_network_id
GROUP BY
	systems.id, 
	systems.code,
	systems.name, 
	systems.location, 
	systems.spot_yield_weight, 
	systems.traffic_order_format, 
	systems.flag, 
	systems.active, 
	systems.effective_date
order by
	systems.code';

print @query;
exec (@query);

DROP TABLE #Network_Temp;

