CREATE PROCEDURE [dbo].[usp_REL_OverrideSpotRateByTrafficAndSystem]
	@system_id int,
	@network_id int,
	@daypart_id int,
	@traffic_detail_id int,
	@ordered_spot_rate money,
	@start_date datetime,
	@end_date datetime
AS
BEGIN
	DECLARE @total_system_subs_per_week INT
	DECLARE @total_system_zones INT
	
	SELECT 
		@total_system_subs_per_week = SUM(tro.subscribers) / COUNT(DISTINCT tro.start_date)
	FROM
		traffic_orders tro (NOLOCK)
	WHERE
		tro.system_id = @system_id
		AND tro.daypart_id = @daypart_id 
		AND tro.display_network_id = @network_id
		AND	(tro.start_date >= @start_date and tro.end_date <= @end_date) 
		AND tro.traffic_detail_id = @traffic_detail_id
	
	SELECT 
		@total_system_zones = COUNT(DISTINCT tro.zone_id)
	FROM
		traffic_orders tro (NOLOCK)
	WHERE
		tro.system_id = @system_id
		AND tro.daypart_id = @daypart_id 
		AND tro.display_network_id = @network_id
		AND	(tro.start_date >= @start_date and tro.end_date <= @end_date) 
		AND tro.traffic_detail_id = @traffic_detail_id  
	
	IF @total_system_subs_per_week  = 0
	BEGIN
		-- fixed/hybred logic
		UPDATE 
			traffic_orders 
		SET 
			traffic_orders.ordered_spot_rate = CAST(@ordered_spot_rate / CAST(@total_system_zones AS FLOAT) AS MONEY)
		WHERE 
			traffic_orders.system_id = @system_id
			AND traffic_orders.daypart_id = @daypart_id 
			AND traffic_orders.display_network_id = @network_id
			AND	
			(
				traffic_orders.start_date >= @start_date and traffic_orders.end_date <= @end_date
			) 
			AND 
			traffic_orders.traffic_detail_id = @traffic_detail_id 
	END
	ELSE
	BEGIN
		-- variable logic
		UPDATE 
			traffic_orders 
		SET 
			traffic_orders.ordered_spot_rate = CAST(@ordered_spot_rate * (CAST(traffic_orders.subscribers AS FLOAT) / CAST(@total_system_subs_per_week AS FLOAT)) AS MONEY)
		WHERE 
			traffic_orders.system_id = @system_id
			AND traffic_orders.daypart_id = @daypart_id 
			AND traffic_orders.display_network_id = @network_id
			AND	
			(
				traffic_orders.start_date >= @start_date and traffic_orders.end_date <= @end_date
			) 
			AND 
			traffic_orders.traffic_detail_id = @traffic_detail_id 
	END
END

