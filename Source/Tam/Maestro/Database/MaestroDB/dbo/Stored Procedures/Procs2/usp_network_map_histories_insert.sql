CREATE PROCEDURE usp_network_map_histories_insert
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
INSERT INTO network_map_histories
(
	network_map_id,
	start_date,
	network_id,
	map_set,
	map_value,
	active,
	flag,
	end_date
)
VALUES
(
	@network_map_id,
	@start_date,
	@network_id,
	@map_set,
	@map_value,
	@active,
	@flag,
	@end_date
)

