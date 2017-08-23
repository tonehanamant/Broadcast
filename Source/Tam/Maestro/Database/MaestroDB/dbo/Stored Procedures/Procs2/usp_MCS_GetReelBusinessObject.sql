-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[usp_MCS_GetReelBusinessObject]
	@reel_id INT
AS
BEGIN
	SELECT
		ra.*
	FROM
		dbo.reel_advertisers ra (NOLOCK)
	WHERE
		ra.reel_id=@reel_id

	
	SELECT
		rm.id,
		rm.reel_id, 
		rm.material_id, 
		rm.cut,
		rm.line_number,
		rm.active,
		dm.*
	FROM
		dbo.reel_materials rm (NOLOCK)
		JOIN dbo.uvw_display_materials dm (NOLOCK) on dm.material_id = rm.material_id
	WHERE
		rm.reel_id=@reel_id
	ORDER BY
		rm.line_number, 
		rm.cut
END
