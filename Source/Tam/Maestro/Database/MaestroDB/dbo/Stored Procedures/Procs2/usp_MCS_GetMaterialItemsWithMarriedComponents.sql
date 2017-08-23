-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_MCS_GetMaterialItemsWithMarriedComponents
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT 
		m.id,
		m.code,
		married.code
	FROM 
		materials m (NOLOCK)
		JOIN material_revisions rv (NOLOCK) ON rv.original_material_id=m.id
		JOIN materials married (NOLOCK) ON married.id=rv.revised_material_id

	UNION ALL

	SELECT
		m.id,
		m.code,
		NULL
	FROM
		materials m (NOLOCK)
	WHERE
		m.id NOT IN (
			SELECT original_material_id FROM material_revisions
		)
END
