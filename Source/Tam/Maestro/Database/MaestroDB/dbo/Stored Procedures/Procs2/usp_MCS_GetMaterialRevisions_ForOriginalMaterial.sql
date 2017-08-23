-- =============================================
-- Author:		Stephen DeFusco
-- ALTER date:  8/23/2010
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_MCS_GetMaterialRevisions_ForOriginalMaterial]
	@material_id INT
AS
BEGIN
    SELECT
		mr.*
	FROM 
		material_revisions mr (NOLOCK)
	WHERE
		mr.original_material_id=@material_id
	ORDER BY
		mr.ordinal
END
