-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_MCS_GetMaterialRevisions_ForMaterials]
	@material_ids VARCHAR(MAX)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT
		mr.*
	FROM 
		material_revisions mr (NOLOCK)
	WHERE
		mr.original_material_id IN (
			SELECT id FROM dbo.SplitIntegers(@material_ids)
		)
END
