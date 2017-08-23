CREATE PROCEDURE usp_zone_businesses_delete
(
	@zone_id		Int,
	@business_id		Int,
	@type		VarChar(15)
)
AS
DELETE FROM zone_businesses WHERE zone_id=@zone_id AND business_id=@business_id AND type=@type
