CREATE PROCEDURE usp_zone_businesses_insert
(
	@zone_id		Int,
	@business_id		Int,
	@type		VarChar(15),
	@effective_date		DateTime
)
AS
INSERT INTO zone_businesses
(
	zone_id,
	business_id,
	type,
	effective_date
)
VALUES
(
	@zone_id,
	@business_id,
	@type,
	@effective_date
)

