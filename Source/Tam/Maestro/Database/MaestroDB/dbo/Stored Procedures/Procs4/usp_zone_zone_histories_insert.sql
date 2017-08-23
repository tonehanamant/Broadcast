CREATE PROCEDURE usp_zone_zone_histories_insert
(
	@primary_zone_id		Int,
	@secondary_zone_id		Int,
	@type		VarChar(15),
	@start_date		DateTime,
	@end_date		DateTime
)
AS
INSERT INTO zone_zone_histories
(
	primary_zone_id,
	secondary_zone_id,
	type,
	start_date,
	end_date
)
VALUES
(
	@primary_zone_id,
	@secondary_zone_id,
	@type,
	@start_date,
	@end_date
)

