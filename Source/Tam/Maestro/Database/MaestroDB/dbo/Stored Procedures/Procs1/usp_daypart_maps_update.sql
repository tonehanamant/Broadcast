CREATE PROCEDURE usp_daypart_maps_update
(
	@id		Int,
	@daypart_id		Int,
	@map_set		VarChar(15),
	@map_value		VarChar(63)
)
AS
UPDATE daypart_maps SET
	daypart_id = @daypart_id,
	map_set = @map_set,
	map_value = @map_value
WHERE
	id = @id

