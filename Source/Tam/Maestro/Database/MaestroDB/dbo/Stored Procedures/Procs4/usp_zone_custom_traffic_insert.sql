	
CREATE PROCEDURE usp_zone_custom_traffic_insert
(
	@zone_id		Int,
	@traffic_factor		Float,
	@effective_date		DateTime
)
AS
BEGIN
INSERT INTO dbo.zone_custom_traffic
(
	zone_id,
	traffic_factor,
	effective_date
)
VALUES
(
	@zone_id,
	@traffic_factor,
	@effective_date
)

END
