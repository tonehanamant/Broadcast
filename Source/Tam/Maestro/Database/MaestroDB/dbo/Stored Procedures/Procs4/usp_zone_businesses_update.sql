CREATE PROCEDURE usp_zone_businesses_update
(
	@zone_id		Int,
	@business_id		Int,
	@type		VarChar(15),
	@effective_date		DateTime
)
AS
UPDATE zone_businesses SET
	effective_date = @effective_date
WHERE
	zone_id = @zone_id AND
	business_id = @business_id AND
	type = @type
