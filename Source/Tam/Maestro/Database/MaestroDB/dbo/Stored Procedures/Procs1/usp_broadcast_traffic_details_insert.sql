CREATE PROCEDURE [dbo].[usp_broadcast_traffic_details_insert]
(
	@id		Int		OUTPUT,
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
INSERT INTO broadcast_traffic_details
(
	broadcast_proposal_detail_id,
	revision,
	system_id,
	zone_id,
	accepted,
	ordered_dollars,
	impressions_spots,
	market_percentage,
	employee_id,
	notes
)
VALUES
(
	@broadcast_proposal_detail_id,
	@revision,
	@system_id,
	@zone_id,
	@accepted,
	@ordered_dollars,
	@impressions_spots,
	@market_percentage,
	@employee_id,
	@notes
)

SELECT
	@id = SCOPE_IDENTITY()

/****** Object:  StoredProcedure [dbo].[usp_broadcast_traffic_details_select]    Script Date: 05/09/2012 11:39:16 ******/
SET ANSI_NULLS ON
