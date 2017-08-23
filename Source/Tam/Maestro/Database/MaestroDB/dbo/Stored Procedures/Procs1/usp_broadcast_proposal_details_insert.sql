CREATE PROCEDURE [dbo].[usp_broadcast_proposal_details_insert]
(
	@id		Int		OUTPUT,
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
INSERT INTO broadcast_proposal_details
(
	broadcast_proposal_id,
	original_broadcast_proposal_detail_id,
	revision,
	budget,
	impressions,
	vig,
	spot_length_id,
	broadcast_daypart_id,
	notes,
	employee_id,
	separation,
	proposal_detail_status_id
)
VALUES
(
	@broadcast_proposal_id,
	@original_broadcast_proposal_detail_id,
	@revision,
	@budget,
	@impressions,
	@vig,
	@spot_length_id,
	@broadcast_daypart_id,
	@notes,
	@employee_id,
	@separation,
	@proposal_detail_status_id
)

SELECT
	@id = SCOPE_IDENTITY()

