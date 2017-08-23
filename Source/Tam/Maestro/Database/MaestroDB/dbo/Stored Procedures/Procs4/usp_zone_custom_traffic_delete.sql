	
CREATE PROCEDURE usp_zone_custom_traffic_delete
(
	@zone_id		Int
)
AS
BEGIN
DELETE FROM dbo.zone_custom_traffic WHERE zone_id=@zone_id
END

