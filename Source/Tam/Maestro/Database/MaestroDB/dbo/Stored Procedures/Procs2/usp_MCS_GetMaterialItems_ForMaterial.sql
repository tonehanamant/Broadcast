

CREATE PROCEDURE [dbo].[usp_MCS_GetMaterialItems_ForMaterial]
	@original_material_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT 
		materials.id,
		materials.code,
		spot_lengths.length 
	FROM 
		materials (NOLOCK)
		JOIN spot_lengths (NOLOCK) ON spot_lengths.id=materials.spot_length_id
		JOIN material_revisions (NOLOCK) ON material_revisions.revised_material_id = materials.id 
	WHERE
		material_revisions.original_material_id=@original_material_id
	ORDER BY
		material_revisions.ordinal
END
