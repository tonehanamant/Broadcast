CREATE PROCEDURE usp_material_revisions_delete
(
	@original_material_id		Int,
	@revised_material_id		Int,
	@ordinal		TinyInt)
AS
DELETE FROM
	material_revisions
WHERE
	original_material_id = @original_material_id
 AND
	revised_material_id = @revised_material_id
 AND
	ordinal = @ordinal
