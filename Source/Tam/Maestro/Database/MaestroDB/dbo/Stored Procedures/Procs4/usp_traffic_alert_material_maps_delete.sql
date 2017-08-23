CREATE PROCEDURE [dbo].[usp_traffic_alert_material_maps_delete]
(
	@id Int)
AS
DELETE FROM traffic_alert_material_maps WHERE id=@id
