CREATE PROCEDURE [dbo].[usp_broadcast_traffic_details_update]
(
	@id		Int,
	@broadcast_proposal_detail_id		Int,
	@revision		Int,
	@system_id		Int,
	@zone_id		Int,
	@accepted		Bit,
	@ordered_dollars		Money,
	@impressions_spots		Float,
	@market_percentage		Int,
	@employee_id		Int,
	@notes		VarChar(2074)
)
AS
UPDATE broadcast_traffic_details SET
	broadcast_proposal_detail_id = @broadcast_proposal_detail_id,
	revision = @revision,
	system_id = @system_id,
	zone_id = @zone_id,
	accepted = @accepted,
	ordered_dollars = @ordered_dollars,
	impressions_spots = @impressions_spots,
	market_percentage = @market_percentage,
	employee_id = @employee_id,
	notes = @notes
WHERE
	id = @id

