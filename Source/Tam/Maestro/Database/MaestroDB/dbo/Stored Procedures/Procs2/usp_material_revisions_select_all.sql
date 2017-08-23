CREATE PROCEDURE usp_material_revisions_select_all
AS
SELECT
	*
FROM
	material_revisions WITH(NOLOCK)
