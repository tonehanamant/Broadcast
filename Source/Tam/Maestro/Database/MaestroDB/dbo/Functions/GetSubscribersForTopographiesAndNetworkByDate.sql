-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 
-- Modified:	12/12/2013 - Stephen DeFusco - Added filter for zone_networks.primary flag.
-- Description:	<Description, ,>
-- =============================================
-- SELECT dbo.GetSubscribersForTopographiesAndNetworkByDate('1,9',1,'7/22/2009')
CREATE FUNCTION [dbo].[GetSubscribersForTopographiesAndNetworkByDate] 
(
	@topography_ids VARCHAR(MAX),
	@network_id INT,
	@effective_date DATETIME
)
RETURNS INT
AS
BEGIN	
	DECLARE @lReturn INT;

	DECLARE @ids AS TABLE (id INT)

	INSERT INTO @ids
		SELECT id FROM dbo.SplitIntegers(@topography_ids)

	SET @lReturn = (
		SELECT 
			SUM(subscribers) AS subscribers
		FROM
			uvw_zone_universe z (NOLOCK)
			INNER JOIN (
				SELECT
					znu.zone_id, 
					znu.network_id,
					MAX(znu.subscribers) AS subscribers 
				FROM 
					uvw_zonenetwork_universe znu (NOLOCK) 
					INNER JOIN uvw_network_universe nu (NOLOCK) ON nu.network_id=znu.network_id
				WHERE 
					znu.start_date<=@effective_date AND (znu.end_date>=@effective_date OR znu.end_date IS NULL)
					AND nu.start_date<=@effective_date AND (nu.end_date>=@effective_date OR nu.end_date IS NULL)
					AND nu.active=1
					AND znu.[primary]=1
					AND nu.network_id=@network_id
				GROUP BY 
					znu.zone_id,
					znu.network_id
			) AS channels ON channels.zone_id=z.zone_id
		WHERE
			z.start_date<=@effective_date AND (z.end_date>=@effective_date OR z.end_date IS NULL)
			AND z.active=1
			AND z.[primary]=1
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
						tzu.start_date<=@effective_date AND (tzu.end_date>=@effective_date OR tzu.end_date IS NULL) 
						AND zu.start_date<=@effective_date AND (zu.end_date>=@effective_date OR zu.end_date IS NULL) 
						AND topography_id IN (SELECT id FROM @ids)
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
						zbu.start_date<=@effective_date AND (zbu.end_date>=@effective_date OR zbu.end_date IS NULL) 
						AND zbu.type='MANAGEDBY'
						AND zbu.business_id IN (
							SELECT 
								tbu.business_id 
							FROM 
								uvw_topography_business_universe tbu (NOLOCK) 
								INNER JOIN uvw_business_universe bu (NOLOCK) ON bu.business_id=tbu.business_id 
							WHERE 
								tbu.start_date<=@effective_date AND (tbu.end_date>=@effective_date OR tbu.end_date IS NULL) 
								AND bu.start_date<=@effective_date AND (bu.end_date>=@effective_date OR bu.end_date IS NULL) 
								AND topography_id IN (SELECT id FROM @ids)
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
						zdu.start_date<=@effective_date AND (zdu.end_date>=@effective_date OR zdu.end_date IS NULL)
						AND zdu.dma_id IN (
							SELECT 
								tdu.dma_id 
							FROM 
								uvw_topography_dma_universe tdu (NOLOCK) 
								INNER JOIN uvw_dma_universe du (NOLOCK) ON du.dma_id=tdu.dma_id
							WHERE 
								tdu.start_date<=@effective_date AND (tdu.end_date>=@effective_date OR tdu.end_date IS NULL) 
								AND du.start_date<=@effective_date AND (du.end_date>=@effective_date OR du.end_date IS NULL) 
								AND topography_id IN (SELECT id FROM @ids)
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
						zsu.start_date<=@effective_date AND (zsu.end_date>=@effective_date OR zsu.end_date IS NULL)
						AND zsu.state_id IN (
							SELECT 
								tsu.state_id 
							FROM 
								uvw_topography_state_universe tsu (NOLOCK) 
								INNER JOIN uvw_state_universe su (NOLOCK) ON su.state_id=tsu.state_id 
							WHERE 
								tsu.start_date<=@effective_date AND (tsu.end_date>=@effective_date OR tsu.end_date IS NULL) 
								AND su.start_date<=@effective_date AND (su.end_date>=@effective_date OR su.end_date IS NULL) 
								AND topography_id IN (SELECT id FROM @ids)
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
						szu.start_date<=@effective_date AND (szu.end_date>=@effective_date OR szu.end_date IS NULL)
						AND szu.system_id IN (
							SELECT 
								tsu.system_id 
							FROM 
								uvw_topography_system_universe tsu (NOLOCK) 
								INNER JOIN uvw_system_universe su (NOLOCK) ON su.system_id=tsu.system_id 
							WHERE 
								tsu.start_date<=@effective_date AND (tsu.end_date>=@effective_date OR tsu.end_date IS NULL) 
								AND su.start_date<=@effective_date AND (su.end_date>=@effective_date OR su.end_date IS NULL) 
								AND topography_id IN (SELECT id FROM @ids)
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
						szu.start_date<=@effective_date AND (szu.end_date>=@effective_date OR szu.end_date IS NULL)
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
										tsgu.start_date<=@effective_date AND (tsgu.end_date>=@effective_date OR tsgu.end_date IS NULL) 
										AND sgu.start_date<=@effective_date AND (sgu.end_date>=@effective_date OR sgu.end_date IS NULL) 
										AND topography_id IN (SELECT id FROM @ids)
										AND tsgu.include=1
										AND sgu.active=1
								)
								AND su.start_date<=@effective_date AND (su.end_date>=@effective_date OR su.end_date IS NULL)
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
						tzu.start_date<=@effective_date AND (tzu.end_date>=@effective_date OR tzu.end_date IS NULL) 
						AND zu.start_date<=@effective_date AND (zu.end_date>=@effective_date OR zu.end_date IS NULL) 
						AND topography_id IN (SELECT id FROM @ids)
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
						zbu.start_date<=@effective_date AND (zbu.end_date>=@effective_date OR zbu.end_date IS NULL) 
						AND zbu.type='MANAGEDBY'
						AND zbu.business_id IN (
							SELECT 
								tbu.business_id 
							FROM 
								uvw_topography_business_universe tbu (NOLOCK) 
								INNER JOIN uvw_business_universe bu (NOLOCK) ON bu.business_id=tbu.business_id 
							WHERE 
								tbu.start_date<=@effective_date AND (tbu.end_date>=@effective_date OR tbu.end_date IS NULL) 
								AND bu.start_date<=@effective_date AND (bu.end_date>=@effective_date OR bu.end_date IS NULL) 
								AND topography_id IN (SELECT id FROM @ids)
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
						zdu.start_date<=@effective_date AND (zdu.end_date>=@effective_date OR zdu.end_date IS NULL)
						AND zdu.dma_id IN (
							SELECT 
								tdu.dma_id 
							FROM 
								uvw_topography_dma_universe tdu (NOLOCK) 
								INNER JOIN uvw_dma_universe du (NOLOCK) ON du.dma_id=tdu.dma_id
							WHERE 
								tdu.start_date<=@effective_date AND (tdu.end_date>=@effective_date OR tdu.end_date IS NULL) 
								AND du.start_date<=@effective_date AND (du.end_date>=@effective_date OR du.end_date IS NULL) 
								AND topography_id IN (SELECT id FROM @ids)
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
						zsu.start_date<=@effective_date AND (zsu.end_date>=@effective_date OR zsu.end_date IS NULL)
						AND zsu.state_id IN (
							SELECT 
								tsu.state_id 
							FROM 
								uvw_topography_state_universe tsu (NOLOCK)
								INNER JOIN uvw_state_universe su (NOLOCK) ON su.state_id=tsu.state_id 
							WHERE 
								tsu.start_date<=@effective_date AND (tsu.end_date>=@effective_date OR tsu.end_date IS NULL) 
								AND su.start_date<=@effective_date AND (su.end_date>=@effective_date OR su.end_date IS NULL) 
								AND topography_id IN (SELECT id FROM @ids)
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
						szu.start_date<=@effective_date AND (szu.end_date>=@effective_date OR szu.end_date IS NULL)
						AND szu.system_id IN (
							SELECT 
								tsu.system_id 
							FROM 
								uvw_topography_system_universe tsu (NOLOCK) 
								INNER JOIN uvw_system_universe su (NOLOCK) ON su.system_id=tsu.system_id 
							WHERE 
								tsu.start_date<=@effective_date AND (tsu.end_date>=@effective_date OR tsu.end_date IS NULL) 
								AND su.start_date<=@effective_date AND (su.end_date>=@effective_date OR su.end_date IS NULL) 
								AND topography_id IN (SELECT id FROM @ids)
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
						szu.start_date<=@effective_date AND (szu.end_date>=@effective_date OR szu.end_date IS NULL)
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
										tsgu.start_date<=@effective_date AND (tsgu.end_date>=@effective_date OR tsgu.end_date IS NULL) 
										AND sgu.start_date<=@effective_date AND (sgu.end_date>=@effective_date OR sgu.end_date IS NULL) 
										AND topography_id IN (SELECT id FROM @ids)
										AND tsgu.include=0
										AND sgu.active=1
								)
								AND su.start_date<=@effective_date AND (su.end_date>=@effective_date OR su.end_date IS NULL)
								AND su.active=1
						)
				)
			)
		)

	RETURN @lReturn
END
