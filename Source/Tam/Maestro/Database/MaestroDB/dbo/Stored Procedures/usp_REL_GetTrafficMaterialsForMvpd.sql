
CREATE PROCEDURE [dbo].[usp_REL_GetTrafficMaterialsForMvpd]  
(  
      @traffic_id int,
      @topography_id int
)
 AS 
-- LOCALS
declare @copycount as int;  
declare @start_date as datetime;  
  
select @start_date = traffic.start_date from traffic(NOLOCK) where traffic.id = @traffic_id;  
  
SELECT @copycount = COUNT(*)  
FROM traffic_materials (NOLOCK)
WHERE traffic_materials.traffic_id = @traffic_id
AND traffic_materials.topography_id = @topography_id
  
IF @copycount > 0  
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
          join reel_advertisers ra WITH (NOLOCK) on ra.reel_id = rm.reel_id 
		and ra.line_number = rm.line_number  
          join traffic_materials_disposition tmd WITH (NOLOCK) on tmd.id = tm.disposition_id  
          join traffic t WITH (NOLOCK) on t.id = tm.traffic_id  
          join traffic_proposals tp WITH (NOLOCK) on t.id = tp.traffic_id  
          join proposals p WITH (NOLOCK) on p.id = tp.proposal_id  
          left join topographies topo WITH (NOLOCK) on topo.id = tm.topography_id  
    WHERE  
          t.id = @traffic_id  
          AND tm.topography_id = @topography_id
    order by  
          tm.sort_order, tm.start_date, tm.end_date asc                 
END  
ELSE  
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
          join reel_advertisers ra WITH (NOLOCK) on ra.reel_id = rm.reel_id and ra.line_number = rm.line_number  
          join traffic_materials_disposition tmd WITH (NOLOCK) on tmd.id = tm.disposition_id  
          join traffic t WITH (NOLOCK) on t.id = tm.traffic_id  
          join traffic_proposals tp WITH (NOLOCK) on t.id = tp.traffic_id  
          join proposals p WITH (NOLOCK) on p.id = tp.proposal_id  
          left join topographies topo WITH (NOLOCK) on topo.id = tm.topography_id  
    WHERE  
          t.id = @traffic_id 
          and tm.topography_id is null  
    order by  
          tm.sort_order, tm.start_date, tm.end_date asc                       
END