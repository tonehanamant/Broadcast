	CREATE FUNCTION [dbo].[GetTotalSpotsTrafficked]  
	(  
	 @traffic_id INT,  
	 @system_id INT,  
	 @media_month_id INT,  
	 @effective_date DATETIME  
	)  
	RETURNS FLOAT  
	AS  
	BEGIN  
	 DECLARE @return FLOAT  
	  
	 SET @return = (  
	  SELECT   
	   CAST(SUM(ordered_spots) AS FLOAT)  
	  FROM   
	   traffic_orders  (NOLOCK)   
	   JOIN media_months (NOLOCK) ON   
		traffic_orders.start_date BETWEEN media_months.start_date AND media_months.end_date  
		AND traffic_orders.end_date BETWEEN media_months.start_date AND media_months.end_date  
	  WHERE   
	   media_months.id=@media_month_id  
	   AND traffic_detail_id IN (  
		SELECT  
		 id  
		FROM  
		 traffic_details (NOLOCK)  
		WHERE  
		 traffic_id=@traffic_id  
	   )  
	   AND   
	   (  
		system_id=@system_id  
		OR  
		system_id IN (  
		 SELECT   
		  system_id  
		 FROM  
		  uvw_systemzone_universe usz (NOLOCK)  
		 WHERE  
		  (usz.start_date<=@effective_date AND (usz.end_date>=@effective_date OR usz.end_date IS NULL))  
		  AND zone_id IN (  
		   SELECT  
			primary_zone_id  
		   FROM  
			uvw_zonezone_universe uzz (NOLOCK)  
		   WHERE  
			(uzz.start_date<=@effective_date AND (uzz.end_date>=@effective_date OR uzz.end_date IS NULL)) 
			AND uzz.[type] = 'ZoneGroup'
			AND secondary_zone_id IN (  
			 SELECT  
			  zone_id  
			 FROM  
			  uvw_systemzone_universe usz2 (NOLOCK)  
			 WHERE  
			  (usz2.start_date<=@effective_date AND (usz2.end_date>=@effective_date OR usz2.end_date IS NULL))  
			  AND usz2.system_id=@system_id  
			)  
		  )  
		)  
	   )  
	 )  
	  
	 RETURN @return  
	END  
