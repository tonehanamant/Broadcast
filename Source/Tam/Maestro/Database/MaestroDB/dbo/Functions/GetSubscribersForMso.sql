-- select *, dbo.GetSubscribersForMso(id,'2/9/2009',1,null),dbo.GetSubscribersForMsoByDate(id,'2/9/2009',1,0,1) from businesses
CREATE FUNCTION [dbo].[GetSubscribersForMso]
(
	@business_id INT,
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
				SUM(subscribers) AS subscribers 
			FROM 
				uvw_zonenetwork_universe zn (NOLOCK)
			WHERE
				zn.zone_id IN 
				(
					SELECT DISTINCT zb.zone_id FROM 
						uvw_zonebusiness_universe zb (NOLOCK)
						INNER JOIN uvw_business_universe b	(NOLOCK) ON b.business_id=zb.business_id
						INNER JOIN uvw_zone_universe z		(NOLOCK) ON z.zone_id=zb.zone_id
					WHERE 
						zb.business_id=@business_id
						AND ((@active_only IS NULL OR @active_only=0) OR z.active=@active_only)
						AND ((@active_only IS NULL OR @active_only=0) OR b.active=@active_only)
						AND ((@effective_date IS NULL AND zb.end_date IS NULL) OR (zb.start_date<=@effective_date AND (zb.end_date>=@effective_date OR zb.end_date IS NULL)))
						AND ((@effective_date IS NULL AND  z.end_date IS NULL) OR ( z.start_date<=@effective_date AND ( z.end_date>=@effective_date OR  z.end_date IS NULL)))
						AND (@type IS NULL OR (@type=0 AND z.[primary]=1) OR (@type=1 AND z.traffic=1) OR (@type=2 AND z.dma=1))
				)
				AND ((@effective_date IS NULL AND zn.end_date IS NULL) OR (zn.start_date<=@effective_date AND (zn.end_date>=@effective_date OR zn.end_date IS NULL)))
				AND (@type IS NULL OR (@type=0 AND zn.[primary]=1) OR (@type=1 AND zn.trafficable=1))
			GROUP BY 
				network_id
		) subscribers 
	)
	RETURN @lReturn
END
