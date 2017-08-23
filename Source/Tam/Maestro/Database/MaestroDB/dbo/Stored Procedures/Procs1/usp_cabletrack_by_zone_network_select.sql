	CREATE PROCEDURE usp_cabletrack_by_zone_network_select
	(
		@syscode varchar(15)
		,@network_code varchar(10)
		,@is_hispanic bit
	)
	AS
	BEGIN
		 SELECT DISTINCT cts.tam_media_month 
			,ISNULL(CASE WHEN @is_hispanic = 1 THEN ROUND(cts.tam_subs * (zdmah.dma_pc/100.0),0) ELSE cts.tam_subs END, 0) AS tam_subs  
			,mm.[start_date]  
			,mm.end_date   
		 FROM ct.cable_track_subs (nolock) cts  
		 INNER JOIN media_months (nolock) mm ON cts.tam_media_month = mm.media_month  
		 INNER JOIN zones (nolock) z ON LTRIM(RTRIM(cts.syscode)) = z.code  
			AND z.code = @syscode  
		 LEFT OUTER JOIN zone_dmas (nolock) zdma ON z.id = zdma.zone_id  
		 LEFT OUTER JOIN zone_dma_h (nolock) zdmah ON zdma.dma_id = zdmah.dma_id  
		 WHERE cts.network_code = @network_code  
		 AND mm.[start_date] >= z.effective_date
		 ORDER BY mm.[start_date] ASC  
	END
