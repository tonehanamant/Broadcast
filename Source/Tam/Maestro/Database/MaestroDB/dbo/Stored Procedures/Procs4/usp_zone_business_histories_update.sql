CREATE PROCEDURE usp_zone_business_histories_update
(
	@zone_id		Int,
	@business_id		Int,
	@start_date		DateTime,
	@type		VarChar(15),
	@end_date		DateTime
)
AS
UPDATE zone_business_histories SET
	end_date = @end_date
WHERE
	zone_id = @zone_id AND
	business_id = @business_id AND
	start_date = @start_date AND
	type = @type
