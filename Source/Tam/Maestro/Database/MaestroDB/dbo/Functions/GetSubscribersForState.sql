-- select *, dbo.GetSubscribersForState(id,'2/9/2009',1,null),dbo.GetSubscribersForStateByDate(id,'2/9/2009',1,0,1) from states
CREATE FUNCTION [dbo].[GetSubscribersForState]
(
	@state_id INT,
	@effective_date DATETIME,	-- NULL=use most recent 
	@active_only BIT,			-- NULL=Active and Non-Active, 0=Non-Active Zones Only, 1=Active Zones Only
	@type TINYINT				-- NULL=Type is ignored, 0=Primary, 1=Traffic, 2=Dma
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
				SUM(subscribers*zs.weight) AS subscribers 
			FROM 
				uvw_zonenetwork_universe zn (NOLOCK)
				INNER JOIN uvw_zonestate_universe zs (NOLOCK) ON zs.zone_id=zn.zone_id
			WHERE
				zn.zone_id IN 
				(
					SELECT DISTINCT 
						zs.zone_id 
					FROM 
						uvw_zonestate_universe zs (NOLOCK)
						INNER JOIN uvw_state_universe s (NOLOCK) ON s.state_id=zs.state_id
						INNER JOIN uvw_zone_universe z	(NOLOCK) ON z.zone_id=zs.zone_id
					WHERE 
						zs.state_id=@state_id
						AND ((@active_only IS NULL OR @active_only=0) OR z.active=@active_only)
						AND ((@active_only IS NULL OR @active_only=0) OR s.active=@active_only)
						AND ((@effective_date IS NULL AND zs.end_date IS NULL) OR (zs.start_date<=@effective_date AND (zs.end_date>=@effective_date OR zs.end_date IS NULL)))
						AND ((@effective_date IS NULL AND  s.end_date IS NULL) OR ( s.start_date<=@effective_date AND ( s.end_date>=@effective_date OR  s.end_date IS NULL)))
						AND ((@effective_date IS NULL AND  z.end_date IS NULL) OR ( z.start_date<=@effective_date AND ( z.end_date>=@effective_date OR  z.end_date IS NULL)))
						AND (@type IS NULL OR (@type=0 AND z.[primary]=1) OR (@type=1 AND z.traffic=1) OR (@type=2 AND z.dma=1))
				)
				AND ((@effective_date IS NULL AND zs.end_date IS NULL) OR (zs.start_date<=@effective_date AND (zs.end_date>=@effective_date OR zs.end_date IS NULL)))
				AND ((@effective_date IS NULL AND zn.end_date IS NULL) OR (zn.start_date<=@effective_date AND (zn.end_date>=@effective_date OR zn.end_date IS NULL)))
				AND (@type IS NULL OR (@type=0 AND zn.[primary]=1) OR (@type=1 AND zn.trafficable=1))
			GROUP BY network_id
		) subscribers 
	)

	RETURN @lReturn
END
