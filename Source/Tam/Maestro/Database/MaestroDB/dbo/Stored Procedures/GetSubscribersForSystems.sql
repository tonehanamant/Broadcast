
-- =============================================
-- Author:		<David Chen>
-- Create date: <3/7/2017>
-- =============================================
CREATE PROCEDURE [dbo].[GetSubscribersForSystems]
(
	@system_id UniqueIdTable READONLY,
	@effective_date DATETIME,	-- NULL=use most recent 
	@active_only BIT,			-- NULL=Active and Non-Active, 0=Non-Active Zones Only, 1=Active Zones Only
	@type TINYINT				-- NULL=Type is ignored, 0=Primary, 1=Traffic, 2=Dma
)
AS
BEGIN
	--select *, dbo.GetSubscribersForSystem(id,'5/5/2009',1,null) from systems
	--DECLARE @system_id UniqueIdTable,
	--@effective_date DATETIME = '5/5/2009',	-- NULL=use most recent 
	--@active_only BIT = 1,			-- NULL=Active and Non-Active, 0=Non-Active Zones Only, 1=Active Zones Only
	--@type TINYINT = null

	--insert into @system_id
	--	SELECT 
	--		id 
	--	FROM 
	--		systems;

	DECLARE @relevant_zones TABLE (zone_id INT NOT NULL, system_id INT NOT NULL, PRIMARY KEY CLUSTERED(zone_id, system_id))
	INSERT INTO @relevant_zones
		SELECT DISTINCT 
			sz.zone_id,
			sz.system_id
		FROM 
			uvw_systemzone_universe sz (NOLOCK)
			INNER JOIN uvw_system_universe s	(NOLOCK) ON s.system_id=sz.system_id
			INNER JOIN uvw_zone_universe z		(NOLOCK)ON z.zone_id=sz.zone_id
			JOIN @system_id si on sz.system_id = si.id
		WHERE 
			((@active_only IS NULL OR @active_only=0) OR z.active=@active_only)
			AND ((@active_only IS NULL OR @active_only=0) OR s.active=@active_only)
			AND ((@effective_date IS NULL AND sz.end_date IS NULL) OR (sz.start_date<=@effective_date AND (sz.end_date>=@effective_date OR sz.end_date IS NULL)))
			AND ((@effective_date IS NULL AND  s.end_date IS NULL) OR ( s.start_date<=@effective_date AND ( s.end_date>=@effective_date OR  s.end_date IS NULL)))
			AND ((@effective_date IS NULL AND  z.end_date IS NULL) OR ( z.start_date<=@effective_date AND ( z.end_date>=@effective_date OR  z.end_date IS NULL)))
			AND (@type IS NULL OR (@type=0 AND z.[primary]=1) OR (@type=1 AND z.traffic=1) OR (@type=2 AND z.dma=1))
		OPTION(RECOMPILE)

	SELECT 
		DISTINCT
		si.id system_id,
		MAX(ns.subscribers) AS subscribers
	FROM
		@system_id si
		left outer join (
			SELECT
				network_id, 
				SUM(subscribers) AS subscribers,
				rz.system_id
			FROM 
				uvw_zonenetwork_universe zn (NOLOCK)
				join @relevant_zones rz on rz.zone_id = zn.zone_id
			WHERE
				((@effective_date IS NULL AND zn.end_date IS NULL) OR (zn.start_date<=@effective_date AND (zn.end_date>=@effective_date OR zn.end_date IS NULL)))
				AND (@type IS NULL OR (@type=0 AND zn.[primary]=1) OR (@type=1 AND zn.trafficable=1))
			GROUP BY 
				network_id,
				rz.system_id
		) ns on ns.system_id = si.id
	GROUP BY
		si.id
	OPTION(RECOMPILE)
END