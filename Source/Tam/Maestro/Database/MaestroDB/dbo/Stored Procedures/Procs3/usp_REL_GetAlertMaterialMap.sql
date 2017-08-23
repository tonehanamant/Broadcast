CREATE Procedure [dbo].[usp_REL_GetAlertMaterialMap]
      (
            @traffic_material_id int,
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
where
	traffic_alert_material_maps.traffic_alert_id = @traffic_alert_id
	and
	traffic_alert_material_maps.traffic_material_id = @traffic_material_id
