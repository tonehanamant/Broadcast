
CREATE Procedure [dbo].[usp_TCS_GetTrafficMaterials]  
      (  
            @id Int  
      )  
AS  
SELECT   
      tm.id,   
      traffic_id,   
      tm.material_id,   
      start_date,   
      end_date,   
      rotation,   
      disposition_id,   
      scheduling,   
      comment,   
      dr_phone,   
      m.sensitive,   
      internal_note_id,   
      external_note_id,   
      topography_id,  
      traffic_alert_spot_location,  
   reel_material_id  
from   
      traffic_materials tm (NOLOCK) 
      join reel_materials rm (NOLOCK) on tm.reel_material_id = rm.id
      join materials m (NOLOCK) on rm.material_id = m.id
where traffic_id = @id

