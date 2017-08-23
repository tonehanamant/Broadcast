
CREATE PROCEDURE [dbo].[usp_STS2_SearchZones]
	@search_text VARCHAR(MAX),
	@effective_date DATETIME
AS
BEGIN

SET @search_text = '%' + @search_text + '%'

(
	SELECT DISTINCT
			uvw_zone_universe.zone_id,
			uvw_zone_universe.code 'zone_code',
			uvw_zone_universe.name,
			uvw_zone_universe.type,
			uvw_zone_universe.[primary],
			uvw_zone_universe.traffic,
			uvw_zone_universe.dma,
			uvw_zone_universe.flag,
			uvw_zone_universe.active,
			uvw_zone_universe.start_date,
			uvw_system_universe.system_id,
			uvw_system_universe.code 'system_code',
			uvw_dma_universe.dma_id,
			uvw_dma_universe.name,
			uvw_zone_universe.end_date,
			uvw_zone_universe.time_zone_id,
			uvw_zone_universe.observe_daylight_savings_time,
			tz.name,
			dbo.GetSubscribersForZone(uvw_zone_universe.zone_id,@effective_date,null,null) 
	FROM uvw_zone_universe (NOLOCK) 
	LEFT JOIN uvw_systemzone_universe (NOLOCK) ON uvw_systemzone_universe.zone_id=uvw_zone_universe.zone_id		AND (uvw_systemzone_universe.start_date<=@effective_date AND (uvw_systemzone_universe.end_date>=@effective_date OR uvw_systemzone_universe.end_date IS NULL)) AND uvw_systemzone_universe.type='BILLING'
	LEFT JOIN uvw_system_universe (NOLOCK) ON uvw_system_universe.system_id=uvw_systemzone_universe.system_id	AND (uvw_system_universe.start_date<=@effective_date AND (uvw_system_universe.end_date>=@effective_date OR uvw_system_universe.end_date IS NULL))
	LEFT JOIN uvw_zonedma_universe (NOLOCK) ON uvw_zonedma_universe.zone_id=uvw_zone_universe.zone_id			AND (uvw_zonedma_universe.start_date<=@effective_date AND (uvw_zonedma_universe.end_date>=@effective_date OR uvw_zonedma_universe.end_date IS NULL))
	LEFT JOIN uvw_dma_universe (NOLOCK) ON uvw_dma_universe.dma_id=uvw_zonedma_universe.dma_id					AND (uvw_dma_universe.start_date<=@effective_date AND (uvw_dma_universe.end_date>=@effective_date OR uvw_dma_universe.end_date IS NULL))
	LEFT JOIN time_zones tz (NOLOCK) ON uvw_zone_universe.time_zone_id=tz.id
	WHERE 
		(uvw_zone_universe.start_date<=@effective_date AND (uvw_zone_universe.end_date>=@effective_date OR uvw_zone_universe.end_date IS NULL))
		AND (uvw_zone_universe.code LIKE @search_text OR uvw_zone_universe.name LIKE @search_text)

	UNION ALL

	SELECT DISTINCT
			zones.id,
			zones.code 'zone_code',
			zones.name,
			zones.type,
			zones.[primary],
			zones.traffic,
			zones.dma,
			zones.flag,
			zones.active,
			zones.effective_date,
			systems.id,
			systems.code 'system_code',
			dmas.id,
			dmas.name,
			null,
			zones.time_zone_id,
			zones.observe_daylight_savings_time,
			tz.name,
			dbo.GetSubscribersForZone(zones.id,@effective_date,null,null) 
	FROM zones (NOLOCK) 
		LEFT JOIN system_zones (NOLOCK) ON system_zones.zone_id=zones.id	AND system_zones.type='BILLING'
		LEFT JOIN systems (NOLOCK) ON systems.id=system_zones.system_id	
		LEFT JOIN zone_dmas (NOLOCK) ON zone_dmas.zone_id=zones.id			
		LEFT JOIN dmas (NOLOCK) ON dmas.id=zone_dmas.dma_id					
		LEFT JOIN time_zones tz (NOLOCK) ON zones.time_zone_id=tz.id
	WHERE 
		zones.id NOT IN (
			SELECT zone_id FROM uvw_zone_universe (NOLOCK) WHERE 
				(uvw_zone_universe.start_date<=@effective_date AND (uvw_zone_universe.end_date>=@effective_date OR uvw_zone_universe.end_date IS NULL))
			)
		AND (zones.code LIKE @search_text OR zones.name LIKE @search_text)
)
ORDER BY system_code,zone_code

END

