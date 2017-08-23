
CREATE PROCEDURE usp_zone_custom_traffic_update
(
	@zone_id		Int,
	@traffic_factor		Float,
	@effective_date		DateTime
)
AS
BEGIN
UPDATE dbo.zone_custom_traffic SET
	traffic_factor = @traffic_factor,
	effective_date = @effective_date
WHERE
	zone_id = @zone_id
END
