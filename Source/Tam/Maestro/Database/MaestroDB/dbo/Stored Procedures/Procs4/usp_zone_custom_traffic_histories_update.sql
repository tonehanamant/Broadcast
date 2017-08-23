	
	
CREATE PROCEDURE usp_zone_custom_traffic_histories_update
(
	@zone_id		Int,
	@start_date		DateTime,
	@traffic_factor		Float,
	@end_date		DateTime
)
AS
BEGIN
UPDATE dbo.zone_custom_traffic_histories SET
	traffic_factor = @traffic_factor,
	end_date = @end_date
WHERE
	zone_id = @zone_id AND
	start_date = @start_date
END
