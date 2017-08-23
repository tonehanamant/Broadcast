CREATE PROCEDURE [dbo].[usp_traffic_alert_material_maps_update]
(
	@id		Int,
	@traffic_alert_id		Int,
	@traffic_material_id		Int,
	@rank		Int
)
AS
UPDATE traffic_alert_material_maps SET
	traffic_alert_id = @traffic_alert_id,
	traffic_material_id = @traffic_material_id,
	rank = @rank
WHERE
	id = @id
