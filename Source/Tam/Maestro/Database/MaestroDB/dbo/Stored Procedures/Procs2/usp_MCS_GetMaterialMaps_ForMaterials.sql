-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_MCS_GetMaterialMaps_ForMaterials]
	@material_ids VARCHAR(MAX)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT
		mm.*
	FROM 
		material_maps mm (NOLOCK)
	WHERE
		mm.material_id IN (
			SELECT id FROM dbo.SplitIntegers(@material_ids)
		)
END
