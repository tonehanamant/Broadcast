CREATE PROCEDURE [dbo].[usp_tam_post_materials_update]
(
	@tam_post_proposal_id		Int,
	@material_id		Int,
	@affidavit_material_id		Int,
	@total_spots		Int
)
AS
UPDATE tam_post_materials SET
	total_spots = @total_spots
WHERE
	tam_post_proposal_id = @tam_post_proposal_id AND
	material_id = @material_id AND
	affidavit_material_id = @affidavit_material_id

