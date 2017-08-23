-- =============================================
-- Author:		Stephen DeFUsco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_MCS_GetMarriedMaterialsByChildMaterialId]
	@material_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT
		*
	FROM
		materials m (NOLOCK)
	WHERE
		m.id IN (
			SELECT original_material_id FROM material_revisions mr (NOLOCK) WHERE mr.revised_material_id = @material_id
						)
	order by code
END