CREATE PROCEDURE usp_tam_post_excluded_affidavits_update
(
	@id		Int,
	@tam_post_id		Int,
	@tam_post_proposal_id		Int,
	@material_id		Int,
	@network_id		Int
)
AS
UPDATE tam_post_excluded_affidavits SET
	tam_post_id = @tam_post_id,
	tam_post_proposal_id = @tam_post_proposal_id,
	material_id = @material_id,
	network_id = @network_id
WHERE
	id = @id

