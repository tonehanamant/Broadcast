CREATE PROCEDURE usp_zone_maps_insert
(
	@id		Int		OUTPUT,
	@zone_id		Int,
	@map_set		VarChar(15),
	@map_value		VarChar(63),
	@weight		Float,
	@flag		TinyInt,
	@active		Bit,
	@effective_date		DateTime
)
AS
INSERT INTO zone_maps
(
	zone_id,
	map_set,
	map_value,
	weight,
	flag,
	active,
	effective_date
)
VALUES
(
	@zone_id,
	@map_set,
	@map_value,
	@weight,
	@flag,
	@active,
	@effective_date
)

SELECT
	@id = SCOPE_IDENTITY()

