CREATE PROCEDURE usp_network_map_histories_update
(
	@network_map_id		Int,
	@start_date		DateTime,
	@network_id		Int,
	@map_set		VarChar(15),
	@map_value		VarChar(63),
	@active		Bit,
	@flag		TinyInt,
	@end_date		DateTime
)
AS
UPDATE network_map_histories SET
	network_id = @network_id,
	map_set = @map_set,
	map_value = @map_value,
	active = @active,
	flag = @flag,
	end_date = @end_date
WHERE
	network_map_id = @network_map_id AND
	start_date = @start_date
