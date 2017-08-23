CREATE PROCEDURE usp_tam_post_excluded_affidavits_insert
(
	@id		Int		OUTPUT,
	@tam_post_id		Int,
	@tam_post_proposal_id		Int,
	@material_id		Int,
	@network_id		Int
)
AS
INSERT INTO tam_post_excluded_affidavits
(
	tam_post_id,
	tam_post_proposal_id,
	material_id,
	network_id
)
VALUES
(
	@tam_post_id,
	@tam_post_proposal_id,
	@material_id,
	@network_id
)

SELECT
	@id = SCOPE_IDENTITY()

