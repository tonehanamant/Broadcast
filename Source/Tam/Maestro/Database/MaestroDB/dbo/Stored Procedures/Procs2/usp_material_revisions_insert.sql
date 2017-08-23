CREATE PROCEDURE usp_material_revisions_insert
(
	@original_material_id		Int,
	@revised_material_id		Int,
	@ordinal		TinyInt
)
AS
INSERT INTO material_revisions
(
	original_material_id,
	revised_material_id,
	ordinal
)
VALUES
(
	@original_material_id,
	@revised_material_id,
	@ordinal
)

