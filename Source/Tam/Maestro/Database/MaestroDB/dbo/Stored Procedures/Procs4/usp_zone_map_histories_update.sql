CREATE PROCEDURE usp_zone_map_histories_update
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
UPDATE zone_map_histories SET
	zone_id = @zone_id,
	map_set = @map_set,
	map_value = @map_value,
	weight = @weight,
	flag = @flag,
	active = @active,
	end_date = @end_date
WHERE
	zone_map_id = @zone_map_id AND
	start_date = @start_date
