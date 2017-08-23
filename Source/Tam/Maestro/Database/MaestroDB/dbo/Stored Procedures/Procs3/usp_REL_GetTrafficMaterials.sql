
CREATE PROCEDURE [dbo].[usp_REL_GetTrafficMaterials]  
(  
      @traffic_id int,  
      @system_id int  
      )  
AS  
  
declare @copycount as int;  
declare @start_date as datetime;  
declare @istrafficalert as int;  
declare @topography_specific as int;  
  
select @start_date = traffic.start_date from traffic(NOLOCK) where traffic.id = @traffic_id;  
  
select @topography_specific = ISNULL(MAX(tm.id), 0)   
      from traffic_materials tm WITH (NOLOCK)  
      join dbo.GetTopographiesContainingSystem(@system_id, @start_date) topos on tm.topography_id = topos.topography_id  
where   
      tm.traffic_id = @traffic_id   
  
--First find out if this order is attached to any alerts  
if(@topography_specific > 0)  
BEGIN  
      select @istrafficalert = ISNULL(MAX(ta.id), 0)   
      from traffic_alerts ta WITH(NOLOCK)   
      join traffic_alert_material_maps tam WITH (NOLOCK) on tam.traffic_alert_id = ta.id  
      join traffic_materials tm WITH (NOLOCK) on tm.id = tam.traffic_material_id  
      join dbo.GetTopographiesContainingSystem(@system_id, @start_date) topos on tm.topography_id = topos.topography_id  
where   
      ta.traffic_id = @traffic_id   
      and ta.traffic_alert_type_id in (1,5); -- Copy change only  
END  
ELSE  
BEGIN  
select @istrafficalert = ISNULL(MAX(ta.id), 0)   
      from traffic_alerts ta WITH(NOLOCK)   
      join traffic_alert_material_maps tam WITH (NOLOCK) on tam.traffic_alert_id = ta.id  
      join traffic_materials tm WITH (NOLOCK) on tm.id = tam.traffic_material_id  
where   
      ta.traffic_id = @traffic_id   
      and tm.topography_id is null  
      and ta.traffic_alert_type_id in (1,5);  
END  
  
  
SELECT @copycount = COUNT(*)  
FROM   
      traffic_materials (NOLOCK)   
      join dbo.GetTopographiesContainingSystem(@system_id, @start_date) topos on traffic_materials.topography_id = topos.topography_id  
WHERE  
      traffic_materials.traffic_id = @traffic_id   
  
IF @copycount > 0  
BEGIN  
  
      IF(@istrafficalert > 0)  
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
                  join traffic_alert_material_maps WITH (NOLOCK) on traffic_alert_material_maps.traffic_material_id = tm.id  
                  join dbo.GetTopographiesContainingSystem(@system_id, @start_date) topos on tm.topography_id = topos.topography_id  
                  join traffic_materials_disposition tmd WITH (NOLOCK) on tmd.id = tm.disposition_id  
                  join traffic t WITH (NOLOCK) on t.id = tm.traffic_id  
                  join traffic_proposals tp WITH (NOLOCK) on t.id = tp.traffic_id  
                  join proposals p WITH (NOLOCK) on p.id = tp.proposal_id  
                  left join topographies topo WITH (NOLOCK) on topo.id = tm.topography_id  
            WHERE  
                  t.id = @traffic_id AND traffic_alert_material_maps.traffic_alert_id = @istrafficalert  
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
                  join dbo.GetTopographiesContainingSystem(@system_id, @start_date) topos on tm.topography_id = topos.topography_id  
                  join traffic_materials_disposition tmd WITH (NOLOCK) on tmd.id = tm.disposition_id  
                  join traffic t WITH (NOLOCK) on t.id = tm.traffic_id  
                  join traffic_proposals tp WITH (NOLOCK) on t.id = tp.traffic_id  
                  join proposals p WITH (NOLOCK) on p.id = tp.proposal_id  
                  left join topographies topo WITH (NOLOCK) on topo.id = tm.topography_id  
            WHERE  
                  t.id = @traffic_id  
            order by  
                  tm.sort_order, tm.start_date, tm.end_date asc                 
      END  
END  
ELSE  
BEGIN  
      IF(@istrafficalert > 0)  
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
                  join traffic_alert_material_maps WITH (NOLOCK) on traffic_alert_material_maps.traffic_material_id = tm.id  
                  join traffic_materials_disposition tmd WITH (NOLOCK) on tmd.id = tm.disposition_id  
                  join traffic t WITH (NOLOCK) on t.id = tm.traffic_id  
                  join traffic_proposals tp WITH (NOLOCK) on t.id = tp.traffic_id  
                  join proposals p WITH (NOLOCK) on p.id = tp.proposal_id  
                  left join topographies topo WITH (NOLOCK) on topo.id = tm.topography_id  
            WHERE  
                  t.id = @traffic_id and tm.topography_id is null AND traffic_alert_material_maps.traffic_alert_id = @istrafficalert  
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
                  t.id = @traffic_id and tm.topography_id is null  
            order by  
                  tm.sort_order, tm.start_date, tm.end_date asc                       
      END  
END
