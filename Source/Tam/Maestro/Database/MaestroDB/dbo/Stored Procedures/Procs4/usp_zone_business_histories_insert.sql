CREATE PROCEDURE usp_zone_business_histories_insert
(
	@zone_id		Int,
	@business_id		Int,
	@start_date		DateTime,
	@type		VarChar(15),
	@end_date		DateTime
)
AS
INSERT INTO zone_business_histories
(
	zone_id,
	business_id,
	start_date,
	type,
	end_date
)
VALUES
(
	@zone_id,
	@business_id,
	@start_date,
	@type,
	@end_date
)

