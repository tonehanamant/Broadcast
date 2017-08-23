CREATE PROCEDURE usp_traffic_proposals_delete
(
	@traffic_id		Int,
	@proposal_id		Int)
AS
DELETE FROM
	traffic_proposals
WHERE
	traffic_id = @traffic_id
 AND
	proposal_id = @proposal_id
