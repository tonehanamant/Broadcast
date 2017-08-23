
CREATE FUNCTION [dbo].[udf_GetTrafficZoneInformationByTopographyAsOf]
(     
      @idTopography as int,
      @dateAsOf as datetime,
      @is_trafficakable bit
)
RETURNS @zone_info TABLE
(
	system_id int,
	zone_id int,
	traffic_network_id int,
	zone_network_id int,
	subscribers int,
	traffic_factor float,
	spot_yield_weight float,
	on_financial_reports bit,
	no_cents_in_spot_rate bit
)
AS
BEGIN

	DECLARE @topographyIds UniqueIdTable
	
	INSERT INTO @topographyIds(id) VALUES (@idTopography);
	
	INSERT INTO @zone_info
	SELECT 
		system_id,
		zone_id,
		traffic_network_id,
		zone_network_id,
		subscribers,
		traffic_factor,
		spot_yield_weight,
		on_financial_reports,
		no_cents_in_spot_rate
	FROM dbo.udf_GetTrafficZoneInformationByTopographiesAsOf(@topographyIds, @dateAsOf, @is_trafficakable);
            
    RETURN;
END
