CREATE PROCEDURE usp_system_zones_insert
(
	@zone_id		Int,
	@system_id		Int,
	@type		VarChar(15),
	@effective_date		DateTime
)
AS
INSERT INTO system_zones
(
	zone_id,
	system_id,
	type,
	effective_date
)
VALUES
(
	@zone_id,
	@system_id,
	@type,
	@effective_date
)

