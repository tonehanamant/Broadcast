CREATE PROCEDURE usp_traffic_details_proposal_details_map_delete
(
	@id Int
)
AS
DELETE FROM traffic_details_proposal_details_map WHERE id=@id
