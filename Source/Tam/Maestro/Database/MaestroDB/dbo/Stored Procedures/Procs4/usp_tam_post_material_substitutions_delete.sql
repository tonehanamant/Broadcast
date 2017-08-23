CREATE PROCEDURE usp_tam_post_material_substitutions_delete
(
	@tam_post_id		Int,
	@material_id		Int,
	@substitute_material_id		Int)
AS
DELETE FROM
	tam_post_material_substitutions
WHERE
	tam_post_id = @tam_post_id
 AND
	material_id = @material_id
 AND
	substitute_material_id = @substitute_material_id
