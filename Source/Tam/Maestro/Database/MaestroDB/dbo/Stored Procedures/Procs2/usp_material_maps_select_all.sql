CREATE PROCEDURE usp_material_maps_select_all
AS
SELECT
	*
FROM
	material_maps WITH(NOLOCK)
