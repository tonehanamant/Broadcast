CREATE PROCEDURE usp_zone_map_histories_insert
(
	@zone_map_id		Int,
	@start_date		DateTime,
	@zone_id		Int,
	@map_set		VarChar(15),
	@map_value		VarChar(63),
	@weight		Float,
	@flag		TinyInt,
	@active		Bit,
	@end_date		DateTime
)
AS
INSERT INTO zone_map_histories
(
	zone_map_id,
	start_date,
	zone_id,
	map_set,
	map_value,
	weight,
	flag,
	active,
	end_date
)
VALUES
(
	@zone_map_id,
	@start_date,
	@zone_id,
	@map_set,
	@map_value,
	@weight,
	@flag,
	@active,
	@end_date
)

