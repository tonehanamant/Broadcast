CREATE PROCEDURE usp_traffic_details_proposal_details_map_update
(
	@id		Int,
	@traffic_detail_id		Int,
	@proposal_detail_id		Int,
	@proposal_rate		Money,
	@proposal_spots		Float
)
AS
UPDATE traffic_details_proposal_details_map SET
	traffic_detail_id = @traffic_detail_id,
	proposal_detail_id = @proposal_detail_id,
	proposal_rate = @proposal_rate,
	proposal_spots = @proposal_spots
WHERE
	id = @id

