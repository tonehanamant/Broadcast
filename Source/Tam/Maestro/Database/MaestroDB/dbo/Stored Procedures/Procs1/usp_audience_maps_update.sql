CREATE PROCEDURE usp_audience_maps_update
(
	@id		Int,
	@audience_id		Int,
	@map_set		VarChar(15),
	@map_value		VarChar(63)
)
AS
UPDATE audience_maps SET
	audience_id = @audience_id,
	map_set = @map_set,
	map_value = @map_value
WHERE
	id = @id

