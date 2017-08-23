
-- usp_REL_GetLatestReelForMaterial 1477
CREATE PROCEDURE [dbo].[usp_REL_GetLatestReelForMaterial]
	@material_id INT
AS

SELECT 
	reels.id, 
	reels.name, 
	materials.id, 
	MAX(reels.date_last_modified)
FROM 
	materials (NOLOCK) 
	JOIN reel_materials (NOLOCK) ON reel_materials.material_id = materials.id
	JOIN reels (NOLOCK) ON reels.id = reel_materials.reel_id 
WHERE 
	materials.id = @material_id
GROUP BY
	reels.id, 
	reels.name, 
	materials.id
