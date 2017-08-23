

CREATE Procedure [dbo].[usp_REL_GetTrafficAlertMaterialsForAlert]
(
	@traffic_alert_id int
)
AS
select 
	traffic_alert_material_maps.id,
	traffic_alert_material_maps.traffic_alert_id,
	traffic_alert_material_maps.traffic_material_id,
	traffic_alert_material_maps.rank
from
	traffic_alert_material_maps (NOLOCK) 
	join traffic_materials (NOLOCK) on traffic_materials.id = traffic_alert_material_maps.traffic_material_id
where
	traffic_alert_material_maps.traffic_alert_id = @traffic_alert_id
order by
	traffic_materials.sort_order
