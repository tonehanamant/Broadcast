CREATE PROCEDURE usp_zone_maps_update
(
	@id		Int,
	@zone_id		Int,
	@map_set		VarChar(15),
	@map_value		VarChar(63),
	@weight		Float,
	@flag		TinyInt,
	@active		Bit,
	@effective_date		DateTime
)
AS
UPDATE zone_maps SET
	zone_id = @zone_id,
	map_set = @map_set,
	map_value = @map_value,
	weight = @weight,
	flag = @flag,
	active = @active,
	effective_date = @effective_date
WHERE
	id = @id

