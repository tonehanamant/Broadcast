-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
--usp_STS2_Zone_IntegrityCheck '12/28/2008'
CREATE PROCEDURE usp_STS2_Zone_IntegrityCheck
	@effective_date DATETIME
AS
BEGIN
	SELECT DISTINCT
			uvw_zone_universe.zone_id,
			uvw_zone_universe.code 'zone_code',
			uvw_zone_universe.name,
			uvw_zone_universe.start_date,
			uvw_zone_universe.end_date,
			--uvw_system_universe.system_id,
			uvw_system_universe.code 'system_code',
			--uvw_dma_universe.dma_id,
			uvw_dma_universe.name 'dma_name',
			--b1.business_id 'ownedby_business_id',
			b1.name 'ownedby_name',
			--b2.business_id 'managedby_business_id',
			b2.name 'managedby_name'
	FROM 
		uvw_zone_universe (NOLOCK) 
		LEFT JOIN uvw_systemzone_universe (NOLOCK) ON uvw_systemzone_universe.zone_id=uvw_zone_universe.zone_id		AND (uvw_systemzone_universe.start_date<=@effective_date AND (uvw_systemzone_universe.end_date>=@effective_date OR uvw_systemzone_universe.end_date IS NULL)) AND uvw_systemzone_universe.type='BILLING'
		LEFT JOIN uvw_system_universe (NOLOCK) ON uvw_system_universe.system_id=uvw_systemzone_universe.system_id	AND (uvw_system_universe.start_date<=@effective_date AND (uvw_system_universe.end_date>=@effective_date OR uvw_system_universe.end_date IS NULL))
		LEFT JOIN uvw_zonedma_universe (NOLOCK) ON uvw_zonedma_universe.zone_id=uvw_zone_universe.zone_id			AND (uvw_zonedma_universe.start_date<=@effective_date AND (uvw_zonedma_universe.end_date>=@effective_date OR uvw_zonedma_universe.end_date IS NULL))
		LEFT JOIN uvw_dma_universe (NOLOCK) ON uvw_dma_universe.dma_id=uvw_zonedma_universe.dma_id					AND (uvw_dma_universe.start_date<=@effective_date AND (uvw_dma_universe.end_date>=@effective_date OR uvw_dma_universe.end_date IS NULL))
		LEFT JOIN uvw_zonebusiness_universe zb1 (NOLOCK) ON zb1.zone_id=uvw_zone_universe.zone_id					AND (zb1.start_date<=@effective_date	AND (zb1.end_date>=@effective_date	OR zb1.end_date IS NULL)) AND zb1.type='OWNEDBY'
		LEFT JOIN uvw_business_universe b1 (NOLOCK) ON b1.business_id=zb1.business_id								AND (b1.start_date<=@effective_date		AND (b1.end_date>=@effective_date	OR b1.end_date IS NULL))
		LEFT JOIN uvw_zonebusiness_universe zb2 (NOLOCK) ON zb2.zone_id=uvw_zone_universe.zone_id					AND (zb2.start_date<=@effective_date	AND (zb2.end_date>=@effective_date	OR zb2.end_date IS NULL)) AND zb2.type='MANAGEDBY'
		LEFT JOIN uvw_business_universe b2 (NOLOCK) ON b2.business_id=zb2.business_id								AND (b2.start_date<=@effective_date		AND (b2.end_date>=@effective_date	OR b2.end_date IS NULL))
	WHERE 
		(uvw_zone_universe.start_date<=@effective_date AND (uvw_zone_universe.end_date>=@effective_date OR uvw_zone_universe.end_date IS NULL))
		AND (
			uvw_system_universe.system_id IS NULL
			OR uvw_dma_universe.dma_id IS NULL
			OR b1.business_id IS NULL
			OR b2.business_id IS NULL
		)
END
