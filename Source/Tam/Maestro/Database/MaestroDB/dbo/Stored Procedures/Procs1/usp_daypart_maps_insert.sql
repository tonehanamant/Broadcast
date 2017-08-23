CREATE PROCEDURE usp_daypart_maps_insert
(
	@id		Int		OUTPUT,
	@daypart_id		Int,
	@map_set		VarChar(15),
	@map_value		VarChar(63)
)
AS
INSERT INTO daypart_maps
(
	daypart_id,
	map_set,
	map_value
)
VALUES
(
	@daypart_id,
	@map_set,
	@map_value
)

SELECT
	@id = SCOPE_IDENTITY()

