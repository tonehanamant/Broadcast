CREATE PROCEDURE usp_material_map_histories_insert
(
	@material_map_id		Int,
	@start_date		DateTime,
	@material_id		Int,
	@map_set		VarChar(15),
	@map_value		VarChar(63),
	@active		Bit,
	@end_date		DateTime
)
AS
INSERT INTO material_map_histories
(
	material_map_id,
	start_date,
	material_id,
	map_set,
	map_value,
	active,
	end_date
)
VALUES
(
	@material_map_id,
	@start_date,
	@material_id,
	@map_set,
	@map_value,
	@active,
	@end_date
)

