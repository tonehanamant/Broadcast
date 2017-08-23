CREATE PROCEDURE usp_tam_post_material_substitutions_insert
(
	@tam_post_id		Int,
	@material_id		Int,
	@substitute_material_id		Int
)
AS
INSERT INTO tam_post_material_substitutions
(
	tam_post_id,
	material_id,
	substitute_material_id
)
VALUES
(
	@tam_post_id,
	@material_id,
	@substitute_material_id
)

