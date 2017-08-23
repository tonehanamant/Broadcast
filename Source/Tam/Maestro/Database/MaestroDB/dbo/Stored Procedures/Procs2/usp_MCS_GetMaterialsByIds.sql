-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_MCS_GetMaterialsByIds]
	@material_ids VARCHAR(MAX)
AS
BEGIN
	SELECT
		m.*
	FROM
		materials m (NOLOCK)
	WHERE
		m.id IN (
			SELECT id FROM dbo.SplitIntegers(@material_ids)
		)
END
