  
  CREATE PROCEDURE [dbo].[usp_REL2_GetTrafficMaterialsAttachedToAlert]   
(  
 @traffic_alert_id int,  
 @traffic_id int  
)  
AS  
BEGIN  
  
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED; --SAME AS NO LOCK  
  
select  
 tm.id,  
 tm.traffic_id,  
 rm.cut,  
 ra.display_name,  
 rm.material_id,  
 rm.reel_id,  
 tm.start_date,  
 tm.end_date,   
 tm.rotation,  
 tmd.disposition,  
 tm.disposition_id,  
 tm.scheduling,  
 tm.comment,  
 m.sensitive,  
 tm.topography_id,  
 topo.code,  
 tm.traffic_alert_spot_location,  
 tm.reel_material_id,  
 tm.sort_order  
from  
 traffic_materials tm WITH (NOLOCK)   
 join reel_materials rm WITH (NOLOCK) on tm.reel_material_id = rm.id
 join materials m WITH (NOLOCK) on rm.material_id = m.id  
 join reel_advertisers ra WITH (NOLOCK) on ra.reel_id = rm.reel_id and ra.line_number = rm.line_number  
 join traffic_materials_disposition tmd WITH (NOLOCK) on tmd.id = tm.disposition_id  
 join traffic t WITH (NOLOCK) on t.id = tm.traffic_id  
 join traffic_alert_material_maps tamm on tm.id = tamm.traffic_material_id  
 left join topographies topo WITH (NOLOCK) on topo.id = tm.topography_id  
WHERE  
 t.id = @traffic_id  
AND   
 tamm.traffic_alert_id = @traffic_alert_id  
order by  
     tm.sort_order, tm.start_date, tm.end_date asc  
  
END;

