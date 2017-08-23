CREATE PROCEDURE [dbo].[usp_traffic_alert_material_maps_select_all]
AS
SELECT
	id,
	traffic_alert_id,
	traffic_material_id,
	rank
FROM
	traffic_alert_material_maps (NOLOCK)
