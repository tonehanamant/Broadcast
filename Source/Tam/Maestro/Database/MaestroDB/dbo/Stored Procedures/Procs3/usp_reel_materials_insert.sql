CREATE PROCEDURE usp_reel_materials_insert
(
	@id		Int		OUTPUT,
	@reel_id		Int,
	@material_id		Int,
	@cut		Int,
	@line_number		TinyInt,
	@active		Bit
)
AS
INSERT INTO reel_materials
(
	reel_id,
	material_id,
	cut,
	line_number,
	active
)
VALUES
(
	@reel_id,
	@material_id,
	@cut,
	@line_number,
	@active
)

SELECT
	@id = SCOPE_IDENTITY()

