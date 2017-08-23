-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 10/7/2010
-- Description:	Retrieves all TamPostMaterialSubstitutions for a given TamPost.
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetTamPostMaterialSubstitutions]
	@tam_post_ids VARCHAR(MAX)
AS
BEGIN
    SELECT
		tpms.*
	FROM
		tam_post_material_substitutions tpms (NOLOCK)
	WHERE
		tpms.tam_post_id IN (
			SELECT id FROM dbo.SplitIntegers(@tam_post_ids)
		)
END
