
CREATE PROCEDURE [dbo].[usp_REL_GetTrafficMaterialsByTrafficAndMaterialID]  
 @traffic_id int,  
 @material_id int  
AS  
select   
 tm.id,   
 tm.traffic_id,   
 tm.material_id,   
 tm.start_date,   
 tm.end_date,   
 tm.rotation,   
 tm.disposition_id,   
 tm.scheduling,   
 tm.comment,   
 tm.dr_phone,   
 m.sensitive,  
 tm.internal_note_id,   
 tm.external_note_id,   
 tm.topography_id,  
 tm.traffic_alert_spot_location,  
 tm.sort_order,  
 tm.reel_material_id  
from traffic_materials tm WITH (NOLOCK)   
JOIN reel_materials rm WITH (NOLOCK) on rm.id = tm.reel_material_id  
JOIN materials m WITH (NOLOCK) on rm.material_id = m.id
where   
 tm.traffic_id = @traffic_id and tm.material_id = @material_id  
order by  
 tm.sort_order
