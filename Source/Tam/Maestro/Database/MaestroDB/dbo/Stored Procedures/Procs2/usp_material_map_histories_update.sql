CREATE PROCEDURE usp_material_map_histories_update
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
UPDATE material_map_histories SET
	material_id = @material_id,
	map_set = @map_set,
	map_value = @map_value,
	active = @active,
	end_date = @end_date
WHERE
	material_map_id = @material_map_id AND
	start_date = @start_date
