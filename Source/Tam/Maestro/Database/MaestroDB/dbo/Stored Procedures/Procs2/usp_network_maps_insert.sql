CREATE PROCEDURE usp_network_maps_insert
(
	@id		Int		OUTPUT,
	@network_id		Int,
	@map_set		VarChar(15),
	@map_value		VarChar(63),
	@active		Bit,
	@flag		TinyInt,
	@effective_date		DateTime
)
AS
INSERT INTO network_maps
(
	network_id,
	map_set,
	map_value,
	active,
	flag,
	effective_date
)
VALUES
(
	@network_id,
	@map_set,
	@map_value,
	@active,
	@flag,
	@effective_date
)

SELECT
	@id = SCOPE_IDENTITY()

