CREATE PROCEDURE usp_material_maps_update
(
	@id		Int,
	@material_id		Int,
	@map_set		VarChar(15),
	@map_value		VarChar(63),
	@active		Bit,
	@effective_date		DateTime
)
AS
UPDATE material_maps SET
	material_id = @material_id,
	map_set = @map_set,
	map_value = @map_value,
	active = @active,
	effective_date = @effective_date
WHERE
	id = @id

