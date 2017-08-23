-- select *, dbo.GetSubscribersForSystem(id,'2/9/2009',1,null),dbo.GetSubscribersForSystemByDate(id,'2/9/2009',1,0,1) from systems
CREATE FUNCTION [dbo].[GetSubscribersForSystemExcludingNetworks]
(
	@system_id INT,
	@effective_date DATETIME,	-- NULL=use most recent 
	@active_only BIT,			-- NULL=Active and Non-Active, 0=Non-Active Zones Only, 1=Active Zones Only
	@type TINYINT,				-- NULL=Type is ignored, 0=Primary, 1=Traffic, 2=Dma
	@excluded_network_ids VARCHAR(MAX)
)
RETURNS INT
AS
BEGIN
	DECLARE @lReturn INT

	SET @lReturn = (
		SELECT MAX(subscribers) AS subs FROM
		(
			SELECT 
				network_id, 
				SUM(subscribers) AS subscribers
			FROM 
				uvw_zonenetwork_universe zn (NOLOCK)
			WHERE
				zn.zone_id IN 
				(
					SELECT 
						DISTINCT sz.zone_id 
					FROM 
						uvw_systemzone_universe sz (NOLOCK)
						INNER JOIN uvw_system_universe s	(NOLOCK) ON s.system_id=sz.system_id
						INNER JOIN uvw_zone_universe z		(NOLOCK)ON z.zone_id=sz.zone_id
					WHERE 
						sz.system_id=@system_id 
						AND ((@active_only IS NULL OR @active_only=0) OR z.active=@active_only)
						AND ((@active_only IS NULL OR @active_only=0) OR s.active=@active_only)
						AND ((@effective_date IS NULL AND sz.end_date IS NULL) OR (sz.start_date<=@effective_date AND (sz.end_date>=@effective_date OR sz.end_date IS NULL)))
						AND ((@effective_date IS NULL AND  s.end_date IS NULL) OR ( s.start_date<=@effective_date AND ( s.end_date>=@effective_date OR  s.end_date IS NULL)))
						AND ((@effective_date IS NULL AND  z.end_date IS NULL) OR ( z.start_date<=@effective_date AND ( z.end_date>=@effective_date OR  z.end_date IS NULL)))
						AND (@type IS NULL OR (@type=0 AND z.[primary]=1) OR (@type=1 AND z.traffic=1) OR (@type=2 AND z.dma=1))
				)
				AND ((@effective_date IS NULL AND zn.end_date IS NULL) OR (zn.start_date<=@effective_date AND (zn.end_date>=@effective_date OR zn.end_date IS NULL)))
				AND (@type IS NULL OR (@type=0 AND zn.[primary]=1) OR (@type=1 AND zn.trafficable=1))
				AND (@excluded_network_ids IS NULL OR network_id NOT IN (
					SELECT id FROM dbo.SplitIntegers(@excluded_network_ids)
				))
			GROUP BY 
				network_id
		) subscribers
	)

	RETURN @lReturn
END
