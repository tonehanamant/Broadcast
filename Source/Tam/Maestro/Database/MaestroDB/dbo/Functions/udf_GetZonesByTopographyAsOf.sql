
-- =============================================
-- Author:		David Sisson
-- Create date: 03/16/2009
-- Description:	Returns system_histories as of specified date.
-- =============================================
CREATE FUNCTION [dbo].[udf_GetZonesByTopographyAsOf]
(	
	@idTopography as int,
	@dateAsOf as datetime
)
RETURNS TABLE
AS
RETURN 
(
	select distinct
		z.zone_id, 
		z.code, 
		z.name, 
		z.type, 
		z.[primary], 
		z.traffic, 
		z.dma, 
		z.flag, 
		z.active,
		@dateAsOf as_of_date
	FROM
		uvw_zone_universe z (NOLOCK)
	WHERE
		z.start_date<=@dateAsOf AND (z.end_date>=@dateAsOf OR z.end_date IS NULL)
		AND z.active=1
		-- INCLUSION OF ZONES (WHERE include=1)
		AND 
		(
			-- INCLUDE TOPOGRAPHY ZONES
			z.zone_id IN (
				SELECT 
					tzu.zone_id 
				FROM 
					uvw_topography_zone_universe tzu (NOLOCK) 
					INNER JOIN uvw_zone_universe zu (NOLOCK) ON zu.zone_id=tzu.zone_id
				WHERE 
					tzu.start_date<=@dateAsOf AND (tzu.end_date>=@dateAsOf OR tzu.end_date IS NULL) 
					AND zu.start_date<=@dateAsOf AND (zu.end_date>=@dateAsOf OR zu.end_date IS NULL) 
					AND topography_id=@idTopography
					AND tzu.include=1
					AND zu.active=1
			)
			-- INCLUDE TOPOGRAPHY BUSINESSES
			OR
			z.zone_id IN (
				SELECT DISTINCT
					zone_id 
				FROM 
					uvw_zonebusiness_universe zbu (NOLOCK) 
				WHERE 
					zbu.start_date<=@dateAsOf AND (zbu.end_date>=@dateAsOf OR zbu.end_date IS NULL) 
					AND zbu.type='OWNEDBY'
					AND zbu.business_id IN (
						SELECT 
							tbu.business_id 
						FROM 
							uvw_topography_business_universe tbu (NOLOCK) 
							INNER JOIN uvw_business_universe bu (NOLOCK) ON bu.business_id=tbu.business_id 
						WHERE 
							tbu.start_date<=@dateAsOf AND (tbu.end_date>=@dateAsOf OR tbu.end_date IS NULL) 
							AND bu.start_date<=@dateAsOf AND (bu.end_date>=@dateAsOf OR bu.end_date IS NULL) 
							AND topography_id=@idTopography
							AND tbu.include=1
							AND bu.active=1
					)
			)
			-- INCLUDE TOPOGRAPHY DMAS
			OR
			z.zone_id IN (
				SELECT DISTINCT 
					zone_id 
				FROM 
					uvw_zonedma_universe zdu (NOLOCK) 
				WHERE 
					zdu.start_date<=@dateAsOf AND (zdu.end_date>=@dateAsOf OR zdu.end_date IS NULL)
					AND zdu.dma_id IN (
						SELECT 
							tdu.dma_id 
						FROM 
							uvw_topography_dma_universe tdu (NOLOCK) 
							INNER JOIN uvw_dma_universe du (NOLOCK) ON du.dma_id=tdu.dma_id
						WHERE 
							tdu.start_date<=@dateAsOf AND (tdu.end_date>=@dateAsOf OR tdu.end_date IS NULL) 
							AND du.start_date<=@dateAsOf AND (du.end_date>=@dateAsOf OR du.end_date IS NULL) 
							AND topography_id=@idTopography
							AND tdu.include=1
							AND du.active=1
					)
			)
			-- INCLUDE TOPOGRAPHY STATES
			OR
			z.zone_id IN (
				SELECT DISTINCT 
					zone_id 
				FROM 
					uvw_zonestate_universe zsu (NOLOCK) 
				WHERE 
					zsu.start_date<=@dateAsOf AND (zsu.end_date>=@dateAsOf OR zsu.end_date IS NULL)
					AND zsu.state_id IN (
						SELECT 
							tsu.state_id 
						FROM 
							uvw_topography_state_universe tsu (NOLOCK) 
							INNER JOIN uvw_state_universe su (NOLOCK) ON su.state_id=tsu.state_id 
						WHERE 
							tsu.start_date<=@dateAsOf AND (tsu.end_date>=@dateAsOf OR tsu.end_date IS NULL) 
							AND su.start_date<=@dateAsOf AND (su.end_date>=@dateAsOf OR su.end_date IS NULL) 
							AND topography_id=@idTopography
							AND tsu.include=1
							AND su.active=1
					)
			)
			-- INCLUDE TOPOGRAPHY SYSTEMS
			OR
			z.zone_id IN (
				SELECT DISTINCT 
					zone_id 
				FROM 
					uvw_systemzone_universe szu (NOLOCK) 
				WHERE 
					szu.start_date<=@dateAsOf AND (szu.end_date>=@dateAsOf OR szu.end_date IS NULL)
					AND szu.system_id IN (
						SELECT 
							tsu.system_id 
						FROM 
							uvw_topography_system_universe tsu (NOLOCK) 
							INNER JOIN uvw_system_universe su (NOLOCK) ON su.system_id=tsu.system_id 
						WHERE 
							tsu.start_date<=@dateAsOf AND (tsu.end_date>=@dateAsOf OR tsu.end_date IS NULL) 
							AND su.start_date<=@dateAsOf AND (su.end_date>=@dateAsOf OR su.end_date IS NULL) 
							AND topography_id=@idTopography
							AND tsu.include=1
							AND su.active=1
					)
			)
			-- INCLUDE TOPOGRAPHY SYSTEM GROUPS
			OR
			z.zone_id IN (
				SELECT DISTINCT 
					zone_id 
				FROM 
					uvw_systemzone_universe szu (NOLOCK) 
				WHERE 
					szu.start_date<=@dateAsOf AND (szu.end_date>=@dateAsOf OR szu.end_date IS NULL)
					AND szu.system_id IN (
						SELECT 
							sgsu.system_id 
						FROM 
							uvw_systemgroupsystem_universe sgsu (NOLOCK)
							INNER JOIN uvw_system_universe su (NOLOCK) ON su.system_id=sgsu.system_id 
						WHERE
							sgsu.system_group_id IN (
								SELECT 
									tsgu.system_group_id 
								FROM 
									uvw_topography_system_group_universe tsgu (NOLOCK) 
									INNER JOIN uvw_systemgroup_universe sgu (NOLOCK) ON sgu.system_group_id=tsgu.system_group_id 
								WHERE 
									tsgu.start_date<=@dateAsOf AND (tsgu.end_date>=@dateAsOf OR tsgu.end_date IS NULL) 
									AND sgu.start_date<=@dateAsOf AND (sgu.end_date>=@dateAsOf OR sgu.end_date IS NULL) 
									AND topography_id=@idTopography
									AND tsgu.include=1
									AND sgu.active=1
							)
							AND su.start_date<=@dateAsOf AND (su.end_date>=@dateAsOf OR su.end_date IS NULL)
							AND su.active=1
					)
			)
		)
		-- EXCLUSION OF ZONES (WHERE include=0)
		AND 
		(	
			-- EXCLUDE TOPOGRAPHY ZONES
			z.zone_id NOT IN (
				SELECT 
					tzu.zone_id 
				FROM 
					uvw_topography_zone_universe tzu (NOLOCK) 
					INNER JOIN uvw_zone_universe zu (NOLOCK) ON zu.zone_id=tzu.zone_id
				WHERE 
					tzu.start_date<=@dateAsOf AND (tzu.end_date>=@dateAsOf OR tzu.end_date IS NULL) 
					AND zu.start_date<=@dateAsOf AND (zu.end_date>=@dateAsOf OR zu.end_date IS NULL) 
					AND topography_id=@idTopography
					AND tzu.include=0
					AND zu.active=1
			)
			-- EXCLUDE TOPOGRAPHY BUSINESSES
			OR
			z.zone_id NOT IN (
				SELECT DISTINCT
					zone_id 
				FROM 
					uvw_zonebusiness_universe zbu (NOLOCK) 
				WHERE 
					zbu.start_date<=@dateAsOf AND (zbu.end_date>=@dateAsOf OR zbu.end_date IS NULL) 
					AND zbu.type='OWNEDBY'
					AND zbu.business_id IN (
						SELECT 
							tbu.business_id 
						FROM 
							uvw_topography_business_universe tbu (NOLOCK) 
							INNER JOIN uvw_business_universe bu (NOLOCK) ON bu.business_id=tbu.business_id 
						WHERE 
							tbu.start_date<=@dateAsOf AND (tbu.end_date>=@dateAsOf OR tbu.end_date IS NULL) 
							AND bu.start_date<=@dateAsOf AND (bu.end_date>=@dateAsOf OR bu.end_date IS NULL) 
							AND topography_id=@idTopography
							AND tbu.include=0
							AND bu.active=1
					)
			)
			-- EXCLUDE TOPOGRAPHY DMAS
			OR
			z.zone_id NOT IN (
				SELECT DISTINCT 
					zone_id 
				FROM 
					uvw_zonedma_universe zdu (NOLOCK) 
				WHERE 
					zdu.start_date<=@dateAsOf AND (zdu.end_date>=@dateAsOf OR zdu.end_date IS NULL)
					AND zdu.dma_id IN (
						SELECT 
							tdu.dma_id 
						FROM 
							uvw_topography_dma_universe tdu (NOLOCK) 
							INNER JOIN uvw_dma_universe du (NOLOCK) ON du.dma_id=tdu.dma_id
						WHERE 
							tdu.start_date<=@dateAsOf AND (tdu.end_date>=@dateAsOf OR tdu.end_date IS NULL) 
							AND du.start_date<=@dateAsOf AND (du.end_date>=@dateAsOf OR du.end_date IS NULL) 
							AND topography_id=@idTopography
							AND tdu.include=0
							AND du.active=1
					)
			)
			-- EXCLUDE TOPOGRAPHY STATES
			OR
			z.zone_id NOT IN (
				SELECT DISTINCT 
					zone_id 
				FROM 
					uvw_zonestate_universe zsu (NOLOCK) 
				WHERE 
					zsu.start_date<=@dateAsOf AND (zsu.end_date>=@dateAsOf OR zsu.end_date IS NULL)
					AND zsu.state_id IN (
						SELECT 
							tsu.state_id 
						FROM 
							uvw_topography_state_universe tsu (NOLOCK)
							INNER JOIN uvw_state_universe su (NOLOCK) ON su.state_id=tsu.state_id 
						WHERE 
							tsu.start_date<=@dateAsOf AND (tsu.end_date>=@dateAsOf OR tsu.end_date IS NULL) 
							AND su.start_date<=@dateAsOf AND (su.end_date>=@dateAsOf OR su.end_date IS NULL) 
							AND topography_id=@idTopography
							AND tsu.include=0
							AND su.active=1
					)
			)
			-- EXCLUDE TOPOGRAPHY SYSTEMS
			OR
			z.zone_id NOT IN (
				SELECT DISTINCT 
					zone_id 
				FROM 
					uvw_systemzone_universe szu (NOLOCK) 
				WHERE 
					szu.start_date<=@dateAsOf AND (szu.end_date>=@dateAsOf OR szu.end_date IS NULL)
					AND szu.system_id IN (
						SELECT 
							tsu.system_id 
						FROM 
							uvw_topography_system_universe tsu (NOLOCK) 
							INNER JOIN uvw_system_universe su (NOLOCK) ON su.system_id=tsu.system_id 
						WHERE 
							tsu.start_date<=@dateAsOf AND (tsu.end_date>=@dateAsOf OR tsu.end_date IS NULL) 
							AND su.start_date<=@dateAsOf AND (su.end_date>=@dateAsOf OR su.end_date IS NULL) 
							AND topography_id=@idTopography
							AND tsu.include=0
							AND su.active=1
					)
			)
			-- EXCLUDE TOPOGRAPHY SYSTEM GROUPS
			OR
			z.zone_id NOT IN (
				SELECT DISTINCT 
					zone_id 
				FROM 
					uvw_systemzone_universe szu (NOLOCK) 
				WHERE 
					szu.start_date<=@dateAsOf AND (szu.end_date>=@dateAsOf OR szu.end_date IS NULL)
					AND szu.system_id IN (
						SELECT 
							sgsu.system_id 
						FROM 
							uvw_systemgroupsystem_universe sgsu (NOLOCK) 
							INNER JOIN uvw_system_universe su (NOLOCK) ON su.system_id=sgsu.system_id 
						WHERE
							sgsu.system_group_id IN (
								SELECT 
									tsgu.system_group_id 
								FROM 
									uvw_topography_system_group_universe tsgu (NOLOCK) 
									INNER JOIN uvw_systemgroup_universe sgu (NOLOCK) ON sgu.system_group_id=tsgu.system_group_id 
								WHERE 
									tsgu.start_date<=@dateAsOf AND (tsgu.end_date>=@dateAsOf OR tsgu.end_date IS NULL) 
									AND sgu.start_date<=@dateAsOf AND (sgu.end_date>=@dateAsOf OR sgu.end_date IS NULL) 
									AND topography_id=@idTopography
									AND tsgu.include=0
									AND sgu.active=1
							)
							AND su.start_date<=@dateAsOf AND (su.end_date>=@dateAsOf OR su.end_date IS NULL)
							AND su.active=1
					)
			)
		)
);

