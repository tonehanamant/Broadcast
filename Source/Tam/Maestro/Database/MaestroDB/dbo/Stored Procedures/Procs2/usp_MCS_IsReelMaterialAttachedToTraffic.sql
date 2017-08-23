Create PROCEDURE [dbo].[usp_MCS_IsReelMaterialAttachedToTraffic]
	@reel_material_id INT
AS
BEGIN
    SELECT
		count(*)
	FROM
		traffic_materials tm WITH (NOLOCK) 
	WHERE
		tm.reel_material_id = @reel_material_id	
END
