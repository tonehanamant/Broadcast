CREATE PROCEDURE [dbo].[usp_tam_post_materials_select]
(
	@tam_post_proposal_id		Int,
	@material_id		Int,
	@affidavit_material_id		Int
)
AS
SELECT
	*
FROM
	tam_post_materials WITH(NOLOCK)
WHERE
	tam_post_proposal_id=@tam_post_proposal_id
	AND
	material_id=@material_id
	AND
	affidavit_material_id=@affidavit_material_id
