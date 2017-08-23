CREATE PROCEDURE [dbo].[usp_traffic_alert_material_maps_insert]
(
	@id		int		OUTPUT,
	@traffic_alert_id		Int,
	@traffic_material_id		Int,
	@rank		Int
)
AS
INSERT INTO traffic_alert_material_maps
(
	traffic_alert_id,
	traffic_material_id,
	rank
)
VALUES
(
	@traffic_alert_id,
	@traffic_material_id,
	@rank
)

SELECT
	@id = SCOPE_IDENTITY()
