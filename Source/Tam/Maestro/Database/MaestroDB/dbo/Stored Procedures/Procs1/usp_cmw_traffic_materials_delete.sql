CREATE PROCEDURE usp_cmw_traffic_materials_delete
(
	@cmw_traffic_id		Int,
	@cmw_material_id		Int)
AS
DELETE FROM
	cmw_traffic_materials
WHERE
	cmw_traffic_id = @cmw_traffic_id
 AND
	cmw_material_id = @cmw_material_id
