	CREATE PROCEDURE [dbo].[usp_STS2_PenetrationReport_GetNetworksByDate]  
	 -- WHEN @type=0 THEN retrieve networks for PRIMARY zones  
	 -- WHEN @type=1 THEN retrieve networks for TRAFFIC zones  
	 -- WHEN @type=2 THEN retrieve networks for DMA zones  
	 -- WHEN @type=NULL THEN retrieve all networks in zones  
	 @type TINYINT,  
	 @effective_date DATETIME  
	AS  
	SELECT DISTINCT  
	 nu.network_id,   
	 nu.start_date,   
	 nu.code,   
	 nu.[name],   
	 nu.active,   
	 nu.flag,   
	 ISNULL(nu.end_date, getdate()),
	 nu.language_id,
	 nu.affiliated_network_id,
	 nu.network_type_id   
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
	ORDER BY  
	 nu.code  
