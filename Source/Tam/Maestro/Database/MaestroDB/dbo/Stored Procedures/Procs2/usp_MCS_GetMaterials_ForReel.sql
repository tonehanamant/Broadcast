-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_MCS_GetMaterials_ForReel]
	@reel_id INT
AS
BEGIN
    SELECT 
		m.*
	FROM 
		materials m (NOLOCK)
	WHERE 
		m.id IN (
			SELECT rm.material_id FROM reel_materials rm (NOLOCK) WHERE rm.reel_id=@reel_id
		)
END
