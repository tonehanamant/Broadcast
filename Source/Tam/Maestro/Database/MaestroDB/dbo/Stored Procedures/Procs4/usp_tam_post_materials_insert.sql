CREATE PROCEDURE [dbo].[usp_tam_post_materials_insert]
(
	@tam_post_proposal_id		Int,
	@material_id		Int,
	@affidavit_material_id		Int,
	@total_spots		Int
)
AS
INSERT INTO tam_post_materials
(
	tam_post_proposal_id,
	material_id,
	affidavit_material_id,
	total_spots
)
VALUES
(
	@tam_post_proposal_id,
	@material_id,
	@affidavit_material_id,
	@total_spots
)
