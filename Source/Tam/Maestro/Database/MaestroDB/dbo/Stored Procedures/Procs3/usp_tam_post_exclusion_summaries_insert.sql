CREATE PROCEDURE usp_tam_post_exclusion_summaries_insert
(
	@id		Int		OUTPUT,
	@tam_post_proposal_id		Int,
	@system_id		Int,
	@network_id		Int,
	@material_id		Int,
	@subscribers		BigInt,
	@units		Float
)
AS
INSERT INTO tam_post_exclusion_summaries
(
	tam_post_proposal_id,
	system_id,
	network_id,
	material_id,
	subscribers,
	units
)
VALUES
(
	@tam_post_proposal_id,
	@system_id,
	@network_id,
	@material_id,
	@subscribers,
	@units
)

SELECT
	@id = SCOPE_IDENTITY()

