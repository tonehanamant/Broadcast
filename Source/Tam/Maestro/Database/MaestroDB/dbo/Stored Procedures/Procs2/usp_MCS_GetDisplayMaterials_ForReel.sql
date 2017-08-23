
CREATE PROCEDURE [dbo].[usp_MCS_GetDisplayMaterials_ForReel]
	@reel_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT 
		dm.*
	FROM 
		uvw_display_materials dm WITH (NOLOCK)
	WHERE 
		dm.material_id IN (
			SELECT distinct rm.material_id FROM reel_materials rm (NOLOCK) WHERE rm.reel_id=@reel_id
	)
	ORDER BY 
		dm.code
END
