CREATE PROCEDURE usp_cmw_traffic_materials_select
(
	@cmw_traffic_id		Int,
	@cmw_material_id		Int
)
AS
SELECT
	*
FROM
	cmw_traffic_materials WITH(NOLOCK)
WHERE
	cmw_traffic_id=@cmw_traffic_id
	AND
	cmw_material_id=@cmw_material_id

