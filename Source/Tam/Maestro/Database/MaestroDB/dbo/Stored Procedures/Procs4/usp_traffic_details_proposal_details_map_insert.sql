CREATE PROCEDURE usp_traffic_details_proposal_details_map_insert
(
	@id		Int		OUTPUT,
	@traffic_detail_id		Int,
	@proposal_detail_id		Int,
	@proposal_rate		Money,
	@proposal_spots		Float
)
AS
INSERT INTO traffic_details_proposal_details_map
(
	traffic_detail_id,
	proposal_detail_id,
	proposal_rate,
	proposal_spots
)
VALUES
(
	@traffic_detail_id,
	@proposal_detail_id,
	@proposal_rate,
	@proposal_spots
)

SELECT
	@id = SCOPE_IDENTITY()

