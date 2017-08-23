CREATE PROCEDURE [dbo].[usp_broadcast_proposal_details_update]
(
	@id		Int,
	@broadcast_proposal_id		Int,
	@original_broadcast_proposal_detail_id		Int,
	@revision		Int,
	@budget		Money,
	@impressions		Int,
	@vig		Float,
	@spot_length_id		Int,
	@broadcast_daypart_id		Int,
	@notes		VarChar(2047),
	@employee_id		Int,
	@separation		SmallInt,
	@proposal_detail_status_id		TinyInt
)
AS
UPDATE broadcast_proposal_details SET
	broadcast_proposal_id = @broadcast_proposal_id,
	original_broadcast_proposal_detail_id = @original_broadcast_proposal_detail_id,
	revision = @revision,
	budget = @budget,
	impressions = @impressions,
	vig = @vig,
	spot_length_id = @spot_length_id,
	broadcast_daypart_id = @broadcast_daypart_id,
	notes = @notes,
	employee_id = @employee_id,
	separation = @separation,
	proposal_detail_status_id = @proposal_detail_status_id
WHERE
	id = @id

