CREATE PROCEDURE usp_zone_zones_insert
(
	@primary_zone_id		Int,
	@secondary_zone_id		Int,
	@type		VarChar(15),
	@effective_date		DateTime
)
AS
INSERT INTO zone_zones
(
	primary_zone_id,
	secondary_zone_id,
	type,
	effective_date
)
VALUES
(
	@primary_zone_id,
	@secondary_zone_id,
	@type,
	@effective_date
)

