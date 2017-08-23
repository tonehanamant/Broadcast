CREATE PROCEDURE usp_release_cpmlink_insert
(
	@id		Int		OUTPUT,
	@traffic_id		Int,
	@proposal_id		Int,
	@weighting_factor		Float
)
AS
INSERT INTO release_cpmlink
(
	traffic_id,
	proposal_id,
	weighting_factor
)
VALUES
(
	@traffic_id,
	@proposal_id,
	@weighting_factor
)

SELECT
	@id = SCOPE_IDENTITY()

