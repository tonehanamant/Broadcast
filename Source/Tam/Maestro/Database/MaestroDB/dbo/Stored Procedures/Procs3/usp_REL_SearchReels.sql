-- =============================================
-- Author:		Joe Jacbos
-- Create date: 9/9/2011
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_REL_SearchReels]
	@reelorisci VARCHAR(max),
	@reel_status_id INT
AS
BEGIN
	IF(@reel_status_id > 0)
		BEGIN
			SELECT 
				DISTINCT r.*
			FROM
				reels r WITH (NOLOCK)
				JOIN reel_materials rm WITH (NOLOCK) on rm.reel_id = r.id
				JOIN uvw_display_materials WITH (NOLOCK) on uvw_display_materials.material_id = rm.material_id
			WHERE
				(r.name LIKE '%' + @reelorisci + '%' OR uvw_display_materials.code LIKE '%' + @reelorisci + '%')
				AND
				r.status_code = @reel_status_id
		END
	ELSE
		BEGIN
			SELECT 
				DISTINCT r.*
			FROM
				reels r WITH (NOLOCK)
				JOIN reel_materials rm WITH (NOLOCK) on rm.reel_id = r.id
				JOIN uvw_display_materials WITH (NOLOCK) on uvw_display_materials.material_id = rm.material_id
			WHERE
				(r.name LIKE '%' + @reelorisci + '%' OR uvw_display_materials.code LIKE '%' + @reelorisci + '%')
		END
END
