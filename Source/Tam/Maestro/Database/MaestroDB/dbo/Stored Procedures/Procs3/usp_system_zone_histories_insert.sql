CREATE PROCEDURE usp_system_zone_histories_insert
(
	@zone_id		Int,
	@system_id		Int,
	@start_date		DateTime,
	@type		VarChar(15),
	@end_date		DateTime
)
AS
INSERT INTO system_zone_histories
(
	zone_id,
	system_id,
	start_date,
	type,
	end_date
)
VALUES
(
	@zone_id,
	@system_id,
	@start_date,
	@type,
	@end_date
)

