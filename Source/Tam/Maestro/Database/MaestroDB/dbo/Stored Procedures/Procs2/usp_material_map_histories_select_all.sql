CREATE PROCEDURE usp_material_map_histories_select_all
AS
SELECT
	*
FROM
	material_map_histories WITH(NOLOCK)
