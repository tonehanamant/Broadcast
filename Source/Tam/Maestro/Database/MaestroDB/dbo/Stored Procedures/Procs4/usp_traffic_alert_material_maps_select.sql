CREATE PROCEDURE [dbo].[usp_traffic_alert_material_maps_select]
(
	@id Int
)
AS
SELECT
	id,
	traffic_alert_id,
	traffic_material_id,
	rank
FROM
	traffic_alert_material_maps (NOLOCK)
WHERE
	id = @id
