CREATE PROCEDURE usp_material_map_histories_delete
(
	@material_map_id		Int,
	@start_date		DateTime)
AS
DELETE FROM
	material_map_histories
WHERE
	material_map_id = @material_map_id
 AND
	start_date = @start_date
