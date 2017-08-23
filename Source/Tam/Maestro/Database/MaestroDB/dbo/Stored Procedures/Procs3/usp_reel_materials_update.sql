CREATE PROCEDURE usp_reel_materials_update
(
	@id		Int,
	@reel_id		Int,
	@material_id		Int,
	@cut		Int,
	@line_number		TinyInt,
	@active		Bit
)
AS
UPDATE reel_materials SET
	reel_id = @reel_id,
	material_id = @material_id,
	cut = @cut,
	line_number = @line_number,
	active = @active
WHERE
	id = @id

