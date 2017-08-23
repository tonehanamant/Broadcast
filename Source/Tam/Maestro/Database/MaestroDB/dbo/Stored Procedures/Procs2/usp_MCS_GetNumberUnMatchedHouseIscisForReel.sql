CREATE PROCEDURE [dbo].[usp_MCS_GetNumberUnMatchedHouseIscisForReel]
      @reel_id INT
AS
BEGIN

select 
      count(*) 
from reel_materials rm WITH (NOLOCK)
      join materials m WITH (NOLOCK) on m.id = rm.material_id
WHERE 
      m.is_house_isci = 1 and rm.active = 1 and m.real_material_id is null
      and reel_id = @reel_id and m.type = 'Original';
END
