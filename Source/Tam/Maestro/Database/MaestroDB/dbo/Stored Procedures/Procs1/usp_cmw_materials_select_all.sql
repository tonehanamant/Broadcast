CREATE PROCEDURE usp_cmw_materials_select_all
AS
SELECT
	*
FROM
	cmw_materials WITH(NOLOCK)
