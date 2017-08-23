	
CREATE PROCEDURE usp_zone_custom_traffic_histories_insert
(
	@zone_id		Int,
	@start_date		DateTime,
	@traffic_factor		Float,
	@end_date		DateTime
)
AS
BEGIN
INSERT INTO dbo.zone_custom_traffic_histories
(
	zone_id,
	start_date,
	traffic_factor,
	end_date
)
VALUES
(
	@zone_id,
	@start_date,
	@traffic_factor,
	@end_date
)

END
