--usp_STS2_PenetrationReport_SelectCoreByDate 0,'10/13/2010'
CREATE PROCEDURE [dbo].[usp_STS2_PenetrationReport_SelectCoreByDate]
	-- WHEN @type=0 THEN retrieve networks for PRIMARY zones
	-- WHEN @type=1 THEN retrieve networks for TRAFFIC zones
	-- WHEN @type=2 THEN retrieve networks for DMA zones
	-- WHEN @type=NULL THEN retrieve all networks in zones
	@type TINYINT,
	@effective_date DATETIME
AS
--DECLARE @effective_date DATETIME
--SET @effective_date= '3/13/2009'

SELECT 
	z.zone_id, 
	b.business_id, 
	s.system_id, 
	d.dma_id,
	ISNULL(st.state_id, 0) [state_id],
	b.[name] [mso], 
	s.code [system], 
	z.code [syscode], 
	z.[name] location, 
	ISNULL(st.code, '') [state], 
	d.[name] [dma], 
	z.dma [inc_dma],
	z.[primary] [inc_proposal],
	z.traffic [inc_traffic],
	channels.subscribers [max subs],
	ISNULL(zst.weight, CAST(1.0 AS FLOAT)) [state_weight]
FROM 
	uvw_zone_universe z (NOLOCK)
	INNER JOIN (
		SELECT
			znu.zone_id, 
			MAX(znu.subscribers) AS subscribers 
		FROM 
			uvw_zonenetwork_universe znu (NOLOCK) 
			INNER JOIN uvw_network_universe nu (NOLOCK) ON nu.network_id=znu.network_id
			INNER JOIN uvw_zone_universe zu (NOLOCK) ON zu.zone_id=znu.zone_id
		WHERE 
			znu.start_date<=@effective_date AND (znu.end_date>=@effective_date OR znu.end_date IS NULL)
			AND nu.start_date<=@effective_date AND (nu.end_date>=@effective_date OR nu.end_date IS NULL)
			AND zu.start_date<=@effective_date AND (zu.end_date>=@effective_date OR zu.end_date IS NULL)
			AND (@type IS NULL OR (@type=0 AND zu.[primary]=1 AND znu.[primary]=1) OR (@type=1 AND zu.traffic=1 AND znu.trafficable=1) OR (@type=2 AND zu.dma=1))
			AND nu.active=1
			AND zu.active=1
		GROUP BY 
			znu.zone_id
	) AS channels ON (channels.zone_id=z.zone_id)
	INNER JOIN  (
		SELECT DISTINCT 
			zone_id, system_id 
		FROM 
			uvw_systemzone_universe (NOLOCK) 
		WHERE 
			start_date<=@effective_date AND (end_date>=@effective_date OR end_date IS NULL)
			AND type IN ('BILLING','TRAFFIC')
	) AS sz ON sz.zone_id=z.zone_id
	INNER JOIN uvw_system_universe s (NOLOCK) ON 
		sz.system_id=s.system_id 
		AND s.start_date<=@effective_date AND (s.end_date>=@effective_date OR s.end_date IS NULL)
	INNER JOIN (
		SELECT
			zone_id, 
			business_id 
		FROM 
			uvw_zonebusiness_universe (NOLOCK) 
		WHERE 
			start_date<=@effective_date AND (end_date>=@effective_date OR end_date IS NULL) 
			AND type='MANAGEDBY'
	) zb ON zb.zone_id=z.zone_id 
	INNER JOIN uvw_business_universe b (NOLOCK) ON 
		zb.business_id=b.business_id
		AND b.start_date<=@effective_date AND (b.end_date>=@effective_date OR b.end_date IS NULL)
	INNER JOIN (
		SELECT 
			zone_id, 
			dma_id 
		FROM 
			uvw_zonedma_universe (NOLOCK) 
		WHERE 
			start_date<=@effective_date AND (end_date>=@effective_date OR end_date IS NULL)
	) zd ON zd.zone_id=z.zone_id 
	INNER JOIN uvw_dma_universe d (NOLOCK) ON 
		zd.dma_id=d.dma_id 
		AND d.start_date<=@effective_date AND (d.end_date>=@effective_date OR d.end_date IS NULL)
	LEFT JOIN (
		SELECT 
			zone_id, 
			state_id, 
			weight 
		FROM 
			uvw_zonestate_universe (NOLOCK) 
		WHERE 
			start_date<=@effective_date AND (end_date>=@effective_date OR end_date IS NULL)
	) zst ON zst.zone_id=z.zone_id 
	LEFT JOIN uvw_state_universe st (NOLOCK) ON 
		zst.state_id=st.state_id 
		AND st.start_date<=@effective_date AND (st.end_date>=@effective_date OR st.end_date IS NULL)
WHERE 
	z.start_date<=@effective_date AND (z.end_date>=@effective_date OR z.end_date IS NULL)
	AND z.active=1
	AND d.active=1
	AND s.active=1
	AND b.active=1
	AND (st.active IS NULL OR st.active=1)
	AND (@type IS NULL OR (@type=0 AND z.[primary]=1) OR (@type=1 AND z.traffic=1) OR (@type=2 AND z.dma=1))
ORDER BY 
	mso,system,syscode,location,dma


SELECT 
	znu.zone_id, 
	znu.network_id, 
	znu.subscribers
FROM 
	uvw_zonenetwork_universe znu (NOLOCK)
	JOIN uvw_zone_universe zu ON zu.zone_id=znu.zone_id
	JOIN uvw_network_universe nu ON nu.network_id=znu.network_id
WHERE 
	zu.start_date<=@effective_date AND (zu.end_date>=@effective_date OR zu.end_date IS NULL)
	AND nu.start_date<=@effective_date AND (nu.end_date>=@effective_date OR nu.end_date IS NULL)
	AND znu.start_date<=@effective_date AND (znu.end_date>=@effective_date OR znu.end_date IS NULL)
	AND (@type IS NULL OR (@type=0 AND zu.[primary]=1 AND znu.[primary]=1) OR (@type=1 AND zu.traffic=1 AND znu.trafficable=1) OR (@type=2 AND zu.dma=1))
	AND nu.active=1
	AND zu.active=1
ORDER BY 
	znu.zone_id
