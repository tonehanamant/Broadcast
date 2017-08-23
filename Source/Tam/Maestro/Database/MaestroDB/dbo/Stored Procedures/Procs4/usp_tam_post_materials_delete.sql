CREATE PROCEDURE [dbo].[usp_tam_post_materials_delete]
(
	@tam_post_proposal_id		Int,
	@material_id		Int,
	@affidavit_material_id		Int)
AS
DELETE FROM
	tam_post_materials
WHERE
	tam_post_proposal_id = @tam_post_proposal_id
 AND
	material_id = @material_id
 AND
	affidavit_material_id = @affidavit_material_id
