CREATE PROCEDURE usp_release_cpmlink_update
(
	@id		Int,
	@traffic_id		Int,
	@proposal_id		Int,
	@weighting_factor		Float
)
AS
UPDATE release_cpmlink SET
	traffic_id = @traffic_id,
	proposal_id = @proposal_id,
	weighting_factor = @weighting_factor
WHERE
	id = @id

