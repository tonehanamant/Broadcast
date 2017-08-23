
CREATE PROCEDURE [dbo].[usp_REL_GetDisplayTrafficMaterial]  
      @tmid int  
AS  
BEGIN
select  
 tm.id,  
 tm.traffic_id,  
 rm.cut,  
 p.advertiser_company_id,  
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
 join reel_advertisers ra WITH (NOLOCK) on ra.line_number = rm.line_number and ra.reel_id = rm.reel_id  
 join traffic_materials_disposition tmd WITH (NOLOCK) on tmd.id = tm.disposition_id  
 join traffic t WITH (NOLOCK) on t.id = tm.traffic_id  
 join traffic_proposals tp WITH (NOLOCK) on t.id = tp.traffic_id  
 join proposals p WITH (NOLOCK) on p.id = tp.proposal_id  
 left join topographies topo WITH (NOLOCK) on topo.id = tm.topography_id  
WHERE  
 tm.id = @tmid  
order by  
     tm.sort_order, tm.start_date, tm.end_date asc  
END
