CREATE PROCEDURE usp_audience_maps_insert
(
	@id		Int		OUTPUT,
	@audience_id		Int,
	@map_set		VarChar(15),
	@map_value		VarChar(63)
)
AS
INSERT INTO audience_maps
(
	audience_id,
	map_set,
	map_value
)
VALUES
(
	@audience_id,
	@map_set,
	@map_value
)

SELECT
	@id = SCOPE_IDENTITY()

