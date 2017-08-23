-- usp_REL_GetTrafficZoneInformationForAllNetworksWithFactorByTrafficForManyTopos 37269
CREATE PROCEDURE [dbo].[usp_REL_GetTrafficZoneInformationForAllNetworksWithFactorByTrafficForManyTopos]
      @traffic_id as int
AS
BEGIN
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED; --same as NOLOCK

	DECLARE @date as datetime;
	DECLARE @topographies UniqueIdTable;

	INSERT @topographies (id)
		EXEC usp_TCS_GetTopographyIDs @traffic_id

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
		dbo.udf_GetTrafficZoneInformationByTopographiesAsOf(@topographies, @date, 1)
	ORDER BY
		system_id,
		zone_id,
		traffic_network_id,
		zone_network_id;
END