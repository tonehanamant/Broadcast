-- select *, dbo.GetSubscribersForDma(id,'2/9/2009',1,null),dbo.GetSubscribersForDmaByDate(id,'2/9/2009',1,0,1) from dmas
CREATE FUNCTION [dbo].[GetSubscribersForDma]
(
	@dma_id INT,
	@effective_date DATETIME,	-- NULL=use most recent 
	@active_only BIT,			-- NULL=Active and Non-Active, 0=Non-Active Zones Only, 1=Active Zones Only
	@type TINYINT				-- NULL=Type is ignored, 0=Primary, 1=Traffic, 2=Dma
)
RETURNS INT
AS
BEGIN
	DECLARE @lReturn INT

	-- SPECIAL CASE:
	IF @dma_id = 211
		RETURN 0;

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
					SELECT DISTINCT zd.zone_id FROM 
						uvw_zonedma_universe zd (NOLOCK)
						INNER JOIN uvw_dma_universe d  (NOLOCK) ON d.dma_id=zd.dma_id
						INNER JOIN uvw_zone_universe z (NOLOCK) ON z.zone_id=zd.zone_id
					WHERE 
						zd.dma_id=@dma_id
						AND ((@active_only IS NULL OR @active_only=0) OR z.active=@active_only)
						AND ((@active_only IS NULL OR @active_only=0) OR d.active=@active_only)
						AND ((@effective_date IS NULL AND d.end_date IS NULL) OR (d.start_date<=@effective_date AND (d.end_date>=@effective_date OR d.end_date IS NULL)))
						AND ((@effective_date IS NULL AND z.end_date IS NULL) OR (z.start_date<=@effective_date AND (z.end_date>=@effective_date OR z.end_date IS NULL)))
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
