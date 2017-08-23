	/****** Object:  StoredProcedure [dbo].[usp_REL_GetTrafficZoneInformationForAllNetworksWithFactorForManyTopos]    Script Date: 03/11/2015 08:50:22 ******/
	CREATE PROCEDURE [dbo].[usp_REL_GetTrafficZoneInformationForAllNetworksWithFactorForManyTopos]
		@topographyIds as UniqueIdTable READONLY,
		@traffic_id as int
	AS
	BEGIN
		DECLARE @date as datetime;
	
		SELECT 
			@date = MIN(traffic_orders.start_date) 
		FROM 
			traffic_orders
			JOIN traffic_details ON traffic_orders.traffic_detail_id = traffic_details.id 
		WHERE 
			traffic_details.traffic_id = @traffic_id 
			AND traffic_orders.ordered_spots > 0;
	
		IF @date is NULL
		BEGIN
			SELECT @date = traffic.start_date FROM traffic WHERE traffic.id = @traffic_id;
		END
	       
		SELECT
			   topography_id,
			   system_id,
			   zone_id,
			   traffic_network_id,
			   zone_network_id,
			   subscribers,
			   traffic_factor,
			   spot_yield_weight,
			   on_financial_reports,
			   no_cents_in_spot_rate
		FROM
			   dbo.udf_GetTrafficZoneInformationByTopographiesAsOf(@topographyIds, @date, 1)
	END
