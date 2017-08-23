
CREATE PROCEDURE [dbo].[usp_BS_DeleteBroadcastProposal]
(
	@proposal_id int
)
AS
BEGIN
	DELETE FROM broadcast_traffic_detail_statelog where broadcast_traffic_detail_id in
	(select btd.id from broadcast_traffic_details btd WITH (NOLOCK) join broadcast_proposal_details bpt WITH (NOLOCK) on btd.broadcast_proposal_detail_id = bpt.id where bpt.broadcast_proposal_id = @proposal_id);
	
	DELETE FROM broadcast_traffic_detail_weeks where broadcast_traffic_detail_id in
	(select btd.id from broadcast_traffic_details btd WITH (NOLOCK) join broadcast_proposal_details bpt WITH (NOLOCK) on btd.broadcast_proposal_detail_id = bpt.id where bpt.broadcast_proposal_id = @proposal_id);

	DELETE FROM broadcast_traffic_details where broadcast_proposal_detail_id in 
	(select bpd.id from broadcast_proposal_details bpd with (NOLOCK) where broadcast_proposal_id = @proposal_id);

	DELETE FROM broadcast_traffic_detail_audiences where broadcast_proposal_detail_id in
	(select bpd.id from broadcast_proposal_details bpd WITH (NOLOCK) where bpd.broadcast_proposal_id = @proposal_id);

	DELETE FROM broadcast_proposal_detail_flights where broadcast_proposal_detail_id in
	(select bpd.id from broadcast_proposal_details bpd WITH (NOLOCK) where bpd.broadcast_proposal_id = @proposal_id);

	DELETE FROM broadcast_proposal_detail_audiences where broadcast_proposal_detail_id in
	(select bpd.id from broadcast_proposal_details bpd WITH (NOLOCK) where bpd.broadcast_proposal_id = @proposal_id);
	
	DELETE FROM broadcast_detail_markets where broadcast_proposal_detail_id in
	(select bpd.id from broadcast_proposal_details bpd WITH (NOLOCK) where bpd.broadcast_proposal_id = @proposal_id);

	DELETE FROM broadcast_proposal_employees where broadcast_proposal_id = @proposal_id;
	
	DELETE FROM broadcast_proposal_details where broadcast_proposal_id = @proposal_id;
	
	DELETE FROM broadcast_proposals where id = @proposal_id;


END
