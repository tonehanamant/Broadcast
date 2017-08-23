﻿-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 
-- Description:	
-- =============================================
-- usp_STS2_selectDisplayZonesForSystemByDate NULL, '7/15/2011', 483
CREATE PROCEDURE [dbo].[usp_STS2_selectDisplayZonesForSystemByDate]
	@active bit,
	@effective_date datetime,
	@system_id int
AS
BEGIN
	(
		SELECT DISTINCT
			z.zone_id,
			z.code 'zone_code',
			z.name,
			z.[type],
			z.[primary],
			z.traffic,
			z.dma,
			z.flag,
			z.active,
			z.start_date,
			s.system_id,
			s.code,
			d.dma_id,
			d.name,
			z.end_date,
			z.time_zone_id,
			z.observe_daylight_savings_time,
			tz.name,
			dbo.GetSubscribersForZone(z.zone_id,@effective_date,null,null) 
		FROM 
			uvw_zone_universe z (NOLOCK) 
			LEFT JOIN uvw_systemzone_universe sz (NOLOCK) ON sz.zone_id=z.zone_id		AND (sz.start_date<=@effective_date AND (sz.end_date>=@effective_date OR sz.end_date IS NULL)) AND sz.[type]='BILLING'
			LEFT JOIN uvw_system_universe s (NOLOCK) ON s.system_id=sz.system_id		AND (s.start_date<=@effective_date AND (s.end_date>=@effective_date OR s.end_date IS NULL))
			LEFT JOIN uvw_zonedma_universe zd (NOLOCK) ON zd.zone_id=z.zone_id			AND (zd.start_date<=@effective_date AND (zd.end_date>=@effective_date OR zd.end_date IS NULL))
			LEFT JOIN uvw_dma_universe d (NOLOCK) ON d.dma_id=zd.dma_id					AND (d.start_date<=@effective_date AND (d.end_date>=@effective_date OR d.end_date IS NULL))
			LEFT JOIN time_zones tz (NOLOCK) ON z.time_zone_id=tz.id
		WHERE 
			(@active IS NULL OR z.active=@active)
			AND (z.start_date<=@effective_date AND (z.end_date>=@effective_date OR z.end_date IS NULL))
			AND z.zone_id IN (
				SELECT sz2.zone_id FROM uvw_systemzone_universe sz2 WHERE sz2.system_id=@system_id AND (sz2.start_date<=@effective_date AND (sz2.end_date>=@effective_date OR sz2.end_date IS NULL))
			)
		
		UNION
		
		SELECT DISTINCT
			z.id,
			z.code 'zone_code',
			z.name,
			z.type,
			z.[primary],
			z.traffic,
			z.dma,
			z.flag,
			z.active,
			z.effective_date,
			s.id,
			s.code 'system_code',
			d.id,
			d.name,
			null,
			z.time_zone_id,
			z.observe_daylight_savings_time,
			tz.name,
			dbo.GetSubscribersForZone(z.id,@effective_date,null,null) 
		FROM 
			zones z (NOLOCK) 
			LEFT JOIN system_zones sz (NOLOCK) ON sz.zone_id=z.id AND sz.[type]='BILLING'
			LEFT JOIN systems s (NOLOCK) ON s.id=sz.system_id	
			LEFT JOIN zone_dmas zd (NOLOCK) ON zd.zone_id=z.id			
			LEFT JOIN dmas d (NOLOCK) ON d.id=zd.dma_id					
			LEFT JOIN time_zones tz (NOLOCK) ON z.time_zone_id=tz.id
		WHERE 
			(@active IS NULL OR z.active=@active)
			AND z.id NOT IN (
				SELECT 
					z.zone_id 
				FROM 
					uvw_zone_universe z (NOLOCK) 
					LEFT JOIN uvw_systemzone_universe sz (NOLOCK) ON sz.zone_id=z.zone_id	AND (sz.start_date<=@effective_date AND (sz.end_date>=@effective_date OR sz.end_date IS NULL))
					LEFT JOIN uvw_system_universe s (NOLOCK) ON s.system_id=sz.system_id	AND (s.start_date<=@effective_date AND (s.end_date>=@effective_date OR s.end_date IS NULL))
				WHERE 
					(@active IS NULL OR z.active=@active)
					AND (z.start_date<=@effective_date AND (z.end_date>=@effective_date OR z.end_date IS NULL))
					AND z.zone_id IN (
						SELECT sz2.zone_id FROM uvw_systemzone_universe sz2 WHERE sz2.system_id=@system_id AND (sz2.start_date<=@effective_date AND (sz2.end_date>=@effective_date OR sz2.end_date IS NULL))
					)
			)
			AND z.id IN (
				SELECT sz2.zone_id FROM uvw_systemzone_universe sz2 WHERE sz2.system_id=@system_id AND (sz2.start_date<=@effective_date AND (sz2.end_date>=@effective_date OR sz2.end_date IS NULL))
			)
	)
	ORDER BY zone_code
END