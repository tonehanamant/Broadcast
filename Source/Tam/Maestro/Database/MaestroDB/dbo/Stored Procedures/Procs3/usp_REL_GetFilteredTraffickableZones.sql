
CREATE PROCEDURE [dbo].[usp_REL_GetFilteredTraffickableZones]
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
	zones.id,
	zones.code, 
	zones.[name], 
	zones.[type], 
	zones.[primary], 
	zones.traffic, 
	zones.dma,
	zones.flag,
 	zones.active,
	zones.effective_date, 
	count(distinct #Network_Temp.traffic_id)
FROM 
	dbo.udf_GetTrafficZoneInformationByTopographyAsOf(' + CAST(@topography_id as varchar(5)) + ', ''' + convert(varchar, @effective_date, 101) + ''', 1) traffic_zones
join 
	zones WITH (NOLOCK) on zones.id = traffic_zones.zone_id
join
	#Network_Temp WITH (NOLOCK) on #Network_Temp.network_id = traffic_zones.traffic_network_id
GROUP BY
	zones.id,
	zones.code, 
	zones.[name], 
	zones.[type], 
	zones.[primary], 
	zones.traffic, 
	zones.dma,
	zones.flag,
 	zones.active,
	zones.effective_date
order by
	zones.code';

print @query;
exec (@query);

DROP TABLE #Network_Temp;

