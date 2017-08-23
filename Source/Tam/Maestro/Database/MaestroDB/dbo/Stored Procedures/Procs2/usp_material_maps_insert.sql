CREATE PROCEDURE usp_material_maps_insert
(
	@id		Int		OUTPUT,
	@material_id		Int,
	@map_set		VarChar(15),
	@map_value		VarChar(63),
	@active		Bit,
	@effective_date		DateTime
)
AS
INSERT INTO material_maps
(
	material_id,
	map_set,
	map_value,
	active,
	effective_date
)
VALUES
(
	@material_id,
	@map_set,
	@map_value,
	@active,
	@effective_date
)

SELECT
	@id = SCOPE_IDENTITY()

