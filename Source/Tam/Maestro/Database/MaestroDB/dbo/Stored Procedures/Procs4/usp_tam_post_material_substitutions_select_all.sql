CREATE PROCEDURE usp_tam_post_material_substitutions_select_all
AS
SELECT
	*
FROM
	tam_post_material_substitutions WITH(NOLOCK)
