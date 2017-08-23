CREATE PROCEDURE [dbo].[usp_REL_GetZoneNumbersForOneSystemByTrafficAndTopography]
	 @idTopography as int,
	 @traffic_id as int,
	 @system_id as int
AS

DECLARE
	@date as datetime,
	@ratetype int;

SELECT 
	@ratetype = count(*)
FROM 
	topography_maps (NOLOCK)
WHERE
	topography_id = @idTopography
	AND (map_value = 'fxd' OR map_value = 'hyb');

if(@ratetype > 0)
BEGIN
	SELECT 
		@date = min(traffic_orders.start_date) 
	FROM 
		traffic_orders (NOLOCK) join traffic_details (NOLOCK) 
			on traffic_orders.traffic_detail_id = traffic_details.id 
	WHERE 
		traffic_details.traffic_id = @traffic_id 
		AND traffic_orders.ordered_spots > 0
		AND traffic_orders.on_financial_reports = 1;

	IF(@date is NULL)
	BEGIN
		SELECT 
			@date = traffic.start_date 
		FROM 
			traffic (NOLOCK)
		WHERE 
			traffic.id = @traffic_id;
	END
	
	SELECT 
		counts.system_id, MAX(counts.network_count) max_network_count
	FROM (
		SELECT
			tzi.system_id,
			zn.network_id,
			COUNT(distinct zn.network_id) network_count
		FROM
			dbo.udf_GetTrafficZoneInformationByTopographyAsOf(@idTopography, @date, 1) tzi
			JOIN dbo.udf_GetZoneNetworksAsOf(@date) zn ON
					  tzi.zone_id = zn.zone_id
		WHERE
			system_id = @system_id 
			AND tzi.on_financial_reports = 1
			AND zn.trafficable = 1
			AND zn.subscribers > 0
		GROUP BY
			tzi.system_id,
			zn.network_id
		HAVING
			COUNT(distinct zn.network_id) > 1
	) AS counts
	GROUP BY 
		counts.system_id
	ORDER BY
		counts.system_id
END
ELSE
BEGIN
	SELECT @system_id, 1
END

