CREATE PROCEDURE usp_tam_post_exclusion_summaries_update
(
	@id		Int,
	@tam_post_proposal_id		Int,
	@system_id		Int,
	@network_id		Int,
	@material_id		Int,
	@subscribers		BigInt,
	@units		Float
)
AS
UPDATE tam_post_exclusion_summaries SET
	tam_post_proposal_id = @tam_post_proposal_id,
	system_id = @system_id,
	network_id = @network_id,
	material_id = @material_id,
	subscribers = @subscribers,
	units = @units
WHERE
	id = @id

